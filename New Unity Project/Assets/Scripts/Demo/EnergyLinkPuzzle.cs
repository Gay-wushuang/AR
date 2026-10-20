using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class EnergyLinkPuzzle : MonoBehaviour
    {
        [SerializeField] private DemoGameState gameState;
        [SerializeField] private Transform player;
        [SerializeField] private Transform powerUnit;
        public static EnergyLinkPuzzle Instance { get; private set; }
        public static bool Active => Instance != null && Instance.IsOpen;
        public bool IsOpen { get; private set; }
        public bool SprintUnlocked { get; private set; }
        public bool SprintEnabled { get; private set; }
        public float SpeedMultiplier => SprintUnlocked && SprintEnabled ? 1.7f : 1f;
        private readonly int[,] endpoints = new int[3,2];
        private static readonly int[,] levelEndpoints = {
            { 0, 9, 8, 17, 16, 24 },
            { 20, 19, 14, 1, 0, 16 },
            { 16, 23, 24, 11, 10, 2 },
            { 14, 21, 20, 1, 6, 4 },
            { 16, 23, 24, 7, 2, 14 },
            { 4, 23, 22, 5, 0, 8 }
        };
        public int CurrentLevel { get; private set; } = -1;
        public bool BoardSolved { get; private set; }
        public const int LevelCount = 6;
        private void ChooseLevel()
        {
            int next = Random.Range(0, CurrentLevel < 0 ? LevelCount : LevelCount - 1);
            if (CurrentLevel >= 0 && next >= CurrentLevel) next++;
            CurrentLevel = next;
            for (int c=0;c<3;c++) for (int e=0;e<2;e++) endpoints[c,e]=levelEndpoints[next,c*2+e];
            BoardSolved=false;
            ClearBoard();
        }
        private readonly List<int>[] paths = { new List<int>(), new List<int>(), new List<int>() };
        private readonly bool[] connected = new bool[3];
        private readonly Color[] colors = { new Color(1f,.35f,.3f), new Color(.25f,.8f,1f), new Color(1f,.8f,.2f) };
        private int dragging = -1;
        private CursorLockMode oldLock;
        private bool oldVisible;
        private Texture2D dot;
        private string boardMessage = "";
        public int CoveredCells => paths.SelectMany(path => path).Distinct().Count();
        public int ConnectedPairs => connected.Count(v => v);
        private bool Playing => gameState.Status == DemoGameState.GameStatus.Playing || gameState.Status == DemoGameState.GameStatus.ReadyToLaunch;
        private void Awake() { Instance = this; }
        private void OnEnable() { gameState.OnReset += ResetRound; }
        private void OnDisable() { gameState.OnReset -= ResetRound; Close(); }
        private void OnDestroy() { if (Instance == this) Instance = null; if (dot != null) Destroy(dot); }
        private void ResetRound() { Close(); SprintUnlocked = SprintEnabled = false; BoardSolved=false; CurrentLevel=-1; ClearBoard(); }
        public void ClearBoard()
        {
            if (BoardSolved) return;
            boardMessage = "";
            foreach (var path in paths) path.Clear();
            for (int i=0;i<3;i++) connected[i]=false;
            dragging=-1;
        }
        public bool TryOpen()
        {
            if (!Playing || DemoMainMenu.PauseMenuOpen || Time.timeScale <= 0 || IsOpen || Vector3.Distance(player.position,powerUnit.position)>3f) return false;
            ChooseLevel();
            oldLock=Cursor.lockState; oldVisible=Cursor.visible; IsOpen=true;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
            return true;
        }
        public void Close()
        {
            if(!IsOpen)return;
            dragging=-1;
            if (!BoardSolved) ClearBoard();
            IsOpen=false;
            if(Playing && !DemoMainMenu.PauseMenuOpen){Cursor.lockState=oldLock;Cursor.visible=oldVisible;}
        }
        private void Update()
        {
            if (!Playing) { Close(); return; }
            if (DemoMainMenu.PauseMenuOpen || Time.timeScale<=0) return;
            var keys=Keyboard.current;
            if(keys==null)return;
            if(!IsOpen && keys.eKey.wasPressedThisFrame) TryOpen();
            if(!IsOpen && SprintUnlocked && (keys.leftShiftKey.wasPressedThisFrame || keys.rightShiftKey.wasPressedThisFrame)) SprintEnabled=!SprintEnabled;
        }
        private void LateUpdate()
        {
            if(IsOpen){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        }
        private int EndpointColor(int cell)
        {
            for(int i=0;i<3;i++)if(endpoints[i,0]==cell || endpoints[i,1]==cell)return i;
            return -1;
        }
        public bool BeginStroke(int cell)
        {
            if(!IsOpen || BoardSolved || DemoMainMenu.PauseMenuOpen)return false;
            int color=EndpointColor(cell);if(color<0)return false;
            EndStroke();paths[color].Clear();connected[color]=false;dragging=color;paths[color].Add(cell);return true;
        }
        public bool ExtendStroke(int cell)
        {
            if(!IsOpen || dragging<0 || cell<0 || cell>=25 || connected[dragging] || DemoMainMenu.PauseMenuOpen)return false;
            var path=paths[dragging];int last=path[path.Count-1];
            if(Mathf.Abs(cell%5-last%5)+Mathf.Abs(cell/5-last/5)!=1 || path.Contains(cell))return false;
            int endpoint=EndpointColor(cell);if(endpoint>=0 && endpoint!=dragging)return false;
            for(int i=0;i<3;i++)if(i!=dragging && paths[i].Contains(cell))return false;
            path.Add(cell);
            if(endpoint==dragging)connected[dragging]=true;
            return true;
        }
        public void EndStroke()
        {
            if(dragging>=0 && !connected[dragging])paths[dragging].Clear();
            dragging=-1;
            if(connected.All(v=>v))
            {
                if (CoveredCells != 25)
                {
                    ClearBoard();
                    boardMessage = "Cover all 25 cells. Board reset - try again.";
                    return;
                }
                BoardSolved=true;
                if(!SprintUnlocked) SprintEnabled=false;
                SprintUnlocked=true;
            }
        }
        private void EnsureDot()
        {
            if(dot!=null)return;
            dot=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)dot.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((15f-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f)))*2)));
            dot.Apply();
        }
        private void OnGUI()
        {
            if(!Playing || DemoMainMenu.PauseMenuOpen)return;
            if(!IsOpen)
            {
                if(Vector3.Distance(player.position,powerUnit.position)<=3f)GUI.Box(new Rect(Screen.width*.5f-190,Screen.height-66,380,26),"[E] Power terminal: link puzzle / sprint upgrade");
                if(SprintUnlocked)GUI.Label(new Rect(Screen.width-250,Screen.height-30,240,24),SprintEnabled?"SPRINT ON - press SHIFT to walk":"SPRINT READY - press SHIFT to run");
                return;
            }
            EnsureDot();
            float width=Mathf.Min(430,Screen.width*.36f), height=Mathf.Min(550,Screen.height-24);
            Rect panel=new Rect(Screen.width-width-12,12,width,height);
            var old=GUI.color;GUI.color=new Color(.025f,.055f,.08f,.18f);GUI.DrawTexture(panel,Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(panel.x+16,panel.y+10,width-32,24),$"POWER LINK {CurrentLevel+1}/6  |  5 x 5",new GUIStyle(GUI.skin.label){fontSize=18,fontStyle=FontStyle.Bold});
            GUI.Label(new Rect(panel.x+16,panel.y+36,width-32,36),"Drag matching letters/colors. No crossings.\nCover ALL 25 cells to earn the reward.");
            float size=Mathf.Min(width-36,height-132), cellSize=size/5;
            Rect board=new Rect(panel.x+(width-size)/2,panel.y+78,size,size);
            for(int cell=0;cell<25;cell++)
            {
                GUI.color=new Color(.15f,.22f,.28f,.22f);GUI.DrawTexture(new Rect(board.x+(cell%5)*cellSize+1,board.y+(cell/5)*cellSize+1,cellSize-2,cellSize-2),Texture2D.whiteTexture);
            }
            for(int c=0;c<3;c++)
            {
                GUI.color=colors[c];var path=paths[c];
                for(int i=1;i<path.Count;i++)
                {
                    Vector2 a=new Vector2(board.x+(path[i-1]%5+.5f)*cellSize,board.y+(path[i-1]/5+.5f)*cellSize);
                    Vector2 b=new Vector2(board.x+(path[i]%5+.5f)*cellSize,board.y+(path[i]/5+.5f)*cellSize);
                    float t=cellSize*.15f;
                    GUI.DrawTexture(new Rect(Mathf.Min(a.x,b.x)-t/2,Mathf.Min(a.y,b.y)-t/2,Mathf.Abs(a.x-b.x)+t,Mathf.Abs(a.y-b.y)+t),Texture2D.whiteTexture);
                }
            }
            for(int cell=0;cell<25;cell++)
            {
                int c=EndpointColor(cell);float d=cellSize*(c<0?.12f:.65f);
                var r=new Rect(board.x+(cell%5+.5f)*cellSize-d/2,board.y+(cell/5+.5f)*cellSize-d/2,d,d);
                GUI.color=c<0?new Color(.5f,.6f,.65f):colors[c];GUI.DrawTexture(r,dot);
                if(c>=0){GUI.color=Color.black;GUI.Label(r,((char)('A'+c)).ToString(),new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold});}
            }
            GUI.color=Color.white;
            var e=Event.current;
            if(e.button==0 && (e.type==EventType.MouseDown || e.type==EventType.MouseDrag) && board.Contains(e.mousePosition))
            {
                int x=Mathf.FloorToInt((e.mousePosition.x-board.x)/cellSize),y=Mathf.FloorToInt((e.mousePosition.y-board.y)/cellSize);
                if(e.type==EventType.MouseDown)BeginStroke(y*5+x);else ExtendStroke(y*5+x);
                e.Use();
            }
            if(e.type==EventType.MouseUp && e.button==0 && dragging>=0){EndStroke();e.Use();}
            GUI.Label(new Rect(panel.x+16,board.yMax+5,width-32,22),BoardSolved?"SUCCESS! SHIFT toggles 1.7x sprint.":string.IsNullOrEmpty(boardMessage) ? $"Pairs {ConnectedPairs}/3 | Cells {CoveredCells}/25 | O2 paused" : boardMessage);
            var oldBackground=GUI.backgroundColor; GUI.backgroundColor=new Color(1f,1f,1f,.25f);
            if(GUI.Button(new Rect(panel.x+16,panel.yMax-32,(width-40)/2,24),"Clear"))ClearBoard();
            if(GUI.Button(new Rect(panel.x+24+(width-40)/2,panel.yMax-32,(width-40)/2,24),BoardSolved?"Claim / Return":"Exit"))Close();
            GUI.backgroundColor=oldBackground;
            GUI.color=old;
        }
    }
}
