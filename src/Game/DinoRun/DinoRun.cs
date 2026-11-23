
using System.Text;

internal enum GameState
{
    Title,
    Playing,
    GameOver
}

internal enum ObstacleType
{
    SmallCactus,
    LargeCactus,
    Bird
}

internal struct Cell
{
    public char Char;
    public ConsoleColor Color;

    public Cell(char ch, ConsoleColor color)
    {
        Char = ch;
        Color = color;
    }
}

internal sealed class Obstacle
{
    public ObstacleType Type { get; set; }
    public double X { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    /// <summary>
    /// 지면 위 몇 칸 떠 있는지 (0이면 지면 접촉, 1 이상이면 공중)
    /// </summary>
    public int Altitude { get; set; }
    public int AnimFrame { get; set; }
    public double AnimTimer { get; set; }
}

internal static class Game
{
    private const int BoardWidth = 80;
    private const int BoardHeight = 22;
    private const int GroundY = 17; // 지면 라인(0-based)
    private const int DinoX = 10;

    // 물리 상수 (칸/초 단위)
    private const double Gravity = 58.0;
    private const double JumpVelocity = -21.5;
    private const double FastFallGravityMultiplier = 2.2;

    private static readonly Random Rng = new Random();

    private static GameState _state = GameState.Title;
    private static bool _running = true;

    // 공룡 상태
    private static double _dinoY;          // 발 위치 기준 Y좌표 (GroundY가 바닥)
    private static double _dinoVy;         // 수직 속도
    private static bool _isGrounded = true;
    private static bool _isDucking = false;
    private static bool _jumpHeld = false;
    private static bool _duckHeld = false;
    private static double _runAnimTimer = 0.0;
    private static int _runFrame = 0;

    // 장애물 및 점수 상태
    private static readonly List<Obstacle> _obstacles = new List<Obstacle>();
    private static double _scrollSpeed = 28.0;
    private static double _spawnCooldown = 1.1;
    private static double _score = 0.0;
    private static int _highScore = 0;
    private static double _groundScrollOffset = 0.0;

    // 더블 버퍼
    private static readonly Cell[,] _frontBuffer = new Cell[BoardHeight, BoardWidth];
    private static readonly Cell[,] _backBuffer = new Cell[BoardHeight, BoardWidth];
    private static bool _forceFullRedraw = true;

    public static void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        TrySetupWindow();
        ResetWorld(GameState.Title);

        const double targetFps = 60.0;
        const double fixedDeltaTime = 1.0 / targetFps;
        DateTime previousTime = DateTime.UtcNow;
        double accumulator = 0.0;

        while (_running)
        {
            DateTime currentTime = DateTime.UtcNow;
            double frameTime = (currentTime - previousTime).TotalSeconds;
            previousTime = currentTime;

            // 창 이동/일시정지 등으로 프레임 간격이 과도하게 커지는 것 방지
            if (frameTime > 0.1)
            {
                frameTime = 0.1;
            }

            accumulator += frameTime;

            while (accumulator >= fixedDeltaTime)
            {
                Input();
                Update(fixedDeltaTime);
                accumulator -= fixedDeltaTime;
            }

            Render();

            int sleepMs = (int)Math.Max(1, (fixedDeltaTime - accumulator) * 1000.0);
            Thread.Sleep(sleepMs);
        }

        Console.ResetColor();
        Console.CursorVisible = true;
        Console.Clear();
    }

    private static void TrySetupWindow()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            int targetW = Math.Max(BoardWidth + 2, Console.LargestWindowWidth > 0 ? Math.Min(Console.LargestWindowWidth, BoardWidth + 2) : BoardWidth + 2);
            int targetH = Math.Max(BoardHeight + 4, Console.LargestWindowHeight > 0 ? Math.Min(Console.LargestWindowHeight, BoardHeight + 4) : BoardHeight + 4);

            if (Console.BufferWidth < targetW) Console.BufferWidth = targetW;
            if (Console.BufferHeight < targetH) Console.BufferHeight = targetH;
            if (Console.WindowWidth < targetW) Console.WindowWidth = targetW;
            if (Console.WindowHeight < targetH) Console.WindowHeight = targetH;
        }
        catch
        {
            // 일부 콘솔 호스트(리다이렉트/특수 터미널)에서 창 크기 변경이 제한될 수 있으므로 무시
        }
    }

    private static void ResetWorld(GameState nextState)
    {
        _state = nextState;
        _dinoY = GroundY;
        _dinoVy = 0.0;
        _isGrounded = true;
        _isDucking = false;
        _jumpHeld = false;
        _duckHeld = false;
        _runAnimTimer = 0.0;
        _runFrame = 0;

        _obstacles.Clear();
        _scrollSpeed = 28.0;
        _spawnCooldown = 1.0;
        _score = 0.0;
        _forceFullRedraw = true;
    }

    private static void Input()
    {
        _jumpHeld = false;
        _duckHeld = false;

        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            ConsoleKey key = keyInfo.Key;

            if (key == ConsoleKey.Q)
            {
                _running = false;
                return;
            }

            switch (_state)
            {
                case GameState.Title:
                    if (key == ConsoleKey.Spacebar || key == ConsoleKey.UpArrow || key == ConsoleKey.Enter)
                    {
                        ResetWorld(GameState.Playing);
                        StartJump();
                    }
                    break;

                case GameState.Playing:
                    if (key == ConsoleKey.Spacebar || key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                    {
                        _jumpHeld = true;
                        if (_isGrounded)
                        {
                            StartJump();
                        }
                    }
                    else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                    {
                        _duckHeld = true;
                    }
                    break;

                case GameState.GameOver:
                    if (key == ConsoleKey.R || key == ConsoleKey.Spacebar || key == ConsoleKey.Enter)
                    {
                        ResetWorld(GameState.Playing);
                        StartJump();
                    }
                    break;
            }
        }
    }

    private static void StartJump()
    {
        _isGrounded = false;
        _isDucking = false;
        _dinoVy = JumpVelocity;
    }

    private static void Update(double dt)
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        // 점수 및 게임 속도 증가
        _score += _scrollSpeed * dt * 0.45;
        int currentScoreInt = (int)_score;
        if (currentScoreInt > _highScore)
        {
            _highScore = currentScoreInt;
        }

        _scrollSpeed = Math.Min(56.0, 28.0 + (_score * 0.045));
        _groundScrollOffset = (_groundScrollOffset + _scrollSpeed * dt) % BoardWidth;

        // 공룡 숙이기 / 점프 / 중력 처리
        if (_isGrounded)
        {
            _isDucking = _duckHeld;
            _runAnimTimer += dt * (_scrollSpeed / 28.0) * 12.0;
            if (_runAnimTimer >= 1.0)
            {
                _runAnimTimer -= 1.0;
                _runFrame = (_runFrame + 1) % 2;
            }
        }
        else
        {
            _isDucking = false;
            double effectiveGravity = _duckHeld ? Gravity * FastFallGravityMultiplier : Gravity;

            // 점프 버튼을 일찍 떼면 상승 탄력을 약간 줄여 높낮이 조절 가능
            if (!_jumpHeld && _dinoVy < -6.0)
            {
                effectiveGravity *= 1.45;
            }

            _dinoVy += effectiveGravity * dt;
            _dinoY += _dinoVy * dt;

            if (_dinoY >= GroundY)
            {
                _dinoY = GroundY;
                _dinoVy = 0.0;
                _isGrounded = true;
                _isDucking = _duckHeld;
            }
        }

        // 장애물 스폰
        _spawnCooldown -= dt;
        if (_spawnCooldown <= 0.0)
        {
            SpawnObstacle();
            double baseInterval = Math.Max(0.58, 1.35 - (_scrollSpeed - 28.0) * 0.022);
            double jitter = Rng.NextDouble() * 0.45;
            _spawnCooldown = baseInterval + jitter;
        }

        // 장애물 이동 및 충돌 검사
        GetDinoHitbox(out int dinoLeft, out int dinoRight, out int dinoTop, out int dinoBottom);

        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            Obstacle obs = _obstacles[i];
            double speedMultiplier = obs.Type == ObstacleType.Bird ? 1.15 : 1.0;
            obs.X -= _scrollSpeed * speedMultiplier * dt;

            if (obs.Type == ObstacleType.Bird)
            {
                obs.AnimTimer += dt * 8.0;
                if (obs.AnimTimer >= 1.0)
                {
                    obs.AnimTimer -= 1.0;
                    obs.AnimFrame = (obs.AnimFrame + 1) % 2;
                }
            }

            GetObstacleHitbox(obs, out int obsLeft, out int obsRight, out int obsTop, out int obsBottom);

            bool overlaps = dinoLeft <= obsRight &&
                            dinoRight >= obsLeft &&
                            dinoTop <= obsBottom &&
                            dinoBottom >= obsTop;

            if (overlaps)
            {
                _state = GameState.GameOver;
                _forceFullRedraw = true;
                return;
            }

            if (obs.X + obs.Width < 0)
            {
                _obstacles.RemoveAt(i);
            }
        }
    }

    private static void SpawnObstacle()
    {
        int roll = Rng.Next(100);
        ObstacleType type;

        if (_score < 80)
        {
            type = roll < 55 ? ObstacleType.SmallCactus : ObstacleType.LargeCactus;
        }
        else
        {
            if (roll < 35) type = ObstacleType.SmallCactus;
            else if (roll < 70) type = ObstacleType.LargeCactus;
            else type = ObstacleType.Bird;
        }

        Obstacle obs = new Obstacle
        {
            Type = type,
            X = BoardWidth - 1
        };

        switch (type)
        {
            case ObstacleType.SmallCactus:
                obs.Width = 2;
                obs.Height = 2;
                obs.Altitude = 0;
                break;

            case ObstacleType.LargeCactus:
                obs.Width = 3;
                obs.Height = 3;
                obs.Altitude = 0;
                break;

            case ObstacleType.Bird:
                obs.Width = 4;
                obs.Height = 1;
                // 0: 지면 근처(점프 필요), 2: 중간 높이(숙이거나 점프), 3: 머리 위(달리기 가능)
                int[] altitudes = { 0, 2, 3 };
                obs.Altitude = altitudes[Rng.Next(altitudes.Length)];
                break;
        }

        _obstacles.Add(obs);
    }

    private static void GetDinoHitbox(out int left, out int right, out int top, out int bottom)
    {
        int footY = (int)Math.Round(_dinoY);
        bottom = footY;

        if (_isDucking && _isGrounded)
        {
            left = DinoX;
            right = DinoX + 3;
            top = footY - 1; // 높이 2칸 (footY - 1 ~ footY)
        }
        else
        {
            left = DinoX;
            right = DinoX + 2;
            top = footY - 3; // 높이 4칸 (footY - 3 ~ footY)
        }
    }

    private static void GetObstacleHitbox(Obstacle obs, out int left, out int right, out int top, out int bottom)
    {
        left = (int)Math.Round(obs.X) + 1;
        right = (int)Math.Round(obs.X) + obs.Width - 1;
        if (right < left) right = left;

        bottom = GroundY - obs.Altitude;
        top = bottom - (obs.Height - 1);
    }

    private static void Render()
    {
        ClearBackBuffer(new Cell(' ', ConsoleColor.Gray));

        // 상단 HUD
        DrawHud();

        // 지면 렌더링
        DrawGround();

        // 장애물 렌더링
        foreach (Obstacle obs in _obstacles)
        {
            DrawObstacle(obs);
        }

        // 공룡 렌더링
        DrawDino();

        // 상태별 오버레이 메시지
        if (_state == GameState.Title)
        {
            DrawCenteredBox(
                "CHROME DINO JUMP",
                "SPACE / UP : 점프    DOWN : 숙이기    Q : 종료",
                ConsoleColor.Cyan
            );
        }
        else if (_state == GameState.GameOver)
        {
            DrawCenteredBox(
                "G A M E   O V E R",
                "R / SPACE / ENTER : 다시 시작    Q : 종료",
                ConsoleColor.Red
            );
        }

        FlushBackBufferToConsole();
    }

    private static void DrawHud()
    {
        string hudText = $"HI {_highScore:00000}   {(int)_score:00000}";
        int startX = BoardWidth - hudText.Length - 2;
        DrawString(startX, 1, hudText, ConsoleColor.Yellow);

        string guide = _state == GameState.Playing
            ? "[SPACE/↑] 점프   [↓] 숙이기   [Q] 종료"
            : "[Q] 종료";
        DrawString(2, 1, guide, ConsoleColor.DarkGray);
    }

    private static void DrawGround()
    {
        int offset = (int)_groundScrollOffset;
        for (int x = 0; x < BoardWidth; x++)
        {
            int worldX = x + offset;
            char groundChar = '─';
            ConsoleColor color = ConsoleColor.DarkGray;

            if (worldX % 17 == 0)
            {
                groundChar = '˙';
                color = ConsoleColor.Gray;
            }
            else if (worldX % 29 == 0)
            {
                groundChar = '·';
                color = ConsoleColor.Gray;
            }

            SetBackCell(x, GroundY + 1, new Cell(groundChar, color));
        }
    }

    private static void DrawDino()
    {
        int footY = (int)Math.Round(_dinoY);
        ConsoleColor dinoColor = _state == GameState.GameOver ? ConsoleColor.Red : ConsoleColor.Green;

        if (_isDucking && _isGrounded)
        {
            // 숙인 공룡 (2행 x 4열)
            char legChar = _runFrame == 0 ? '┘' : '└';
            DrawString(DinoX, footY - 1, "▄███", dinoColor);
            DrawString(DinoX, footY, $"▀█{legChar} ", dinoColor);
        }
        else
        {
            // 서 있거나 점프 중인 공룡 (4행 x 3열)
            DrawString(DinoX, footY - 3, " █▄", dinoColor);
            DrawString(DinoX, footY - 2, " ██", dinoColor);
            DrawString(DinoX, footY - 1, "▄█▀", dinoColor);

            if (!_isGrounded)
            {
                DrawString(DinoX, footY, "┘└ ", dinoColor);
            }
            else
            {
                string legs = _runFrame == 0 ? "┘  " : " └ ";
                DrawString(DinoX, footY, legs, dinoColor);
            }
        }
    }

    private static void DrawObstacle(Obstacle obs)
    {
        int x = (int)Math.Round(obs.X);
        int bottomY = GroundY - obs.Altitude;

        switch (obs.Type)
        {
            case ObstacleType.SmallCactus:
                DrawString(x, bottomY - 1, " █", ConsoleColor.DarkGreen);
                DrawString(x, bottomY, "██", ConsoleColor.DarkGreen);
                break;

            case ObstacleType.LargeCactus:
                DrawString(x, bottomY - 2, " █ ", ConsoleColor.DarkGreen);
                DrawString(x, bottomY - 1, "███", ConsoleColor.DarkGreen);
                DrawString(x, bottomY, " █ ", ConsoleColor.DarkGreen);
                break;

            case ObstacleType.Bird:
                string wing = obs.AnimFrame == 0 ? "▀" : "▄";
                DrawString(x, bottomY, $"<─{wing}>", ConsoleColor.Magenta);
                break;
        }
    }

    private static void DrawCenteredBox(string title, string subtitle, ConsoleColor borderColor)
    {
        int boxWidth = Math.Max(title.Length, subtitle.Length) + 8;
        int boxHeight = 5;
        int left = (BoardWidth - boxWidth) / 2;
        int top = (GroundY - boxHeight) / 2;

        Cell empty = new Cell(' ', ConsoleColor.White);
        for (int y = top; y < top + boxHeight; y++)
        {
            for (int x = left; x < left + boxWidth; x++)
            {
                SetBackCell(x, y, empty);
            }
        }

        SetBackCell(left, top, new Cell('┌', borderColor));
        SetBackCell(left + boxWidth - 1, top, new Cell('┐', borderColor));
        SetBackCell(left, top + boxHeight - 1, new Cell('└', borderColor));
        SetBackCell(left + boxWidth - 1, top + boxHeight - 1, new Cell('┘', borderColor));

        for (int x = left + 1; x < left + boxWidth - 1; x++)
        {
            SetBackCell(x, top, new Cell('─', borderColor));
            SetBackCell(x, top + boxHeight - 1, new Cell('─', borderColor));
        }

        for (int y = top + 1; y < top + boxHeight - 1; y++)
        {
            SetBackCell(left, y, new Cell('│', borderColor));
            SetBackCell(left + boxWidth - 1, y, new Cell('│', borderColor));
        }

        int titleX = left + (boxWidth - title.Length) / 2;
        int subX = left + (boxWidth - subtitle.Length) / 2;

        DrawString(titleX, top + 1, title, borderColor);
        DrawString(subX, top + 3, subtitle, ConsoleColor.White);
    }

    private static void ClearBackBuffer(Cell fill)
    {
        for (int y = 0; y < BoardHeight; y++)
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                _backBuffer[y, x] = fill;
            }
        }
    }

    private static void SetBackCell(int x, int y, Cell cell)
    {
        if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight)
        {
            return;
        }
        _backBuffer[y, x] = cell;
    }

    private static void DrawString(int x, int y, string text, ConsoleColor color)
    {
        if (y < 0 || y >= BoardHeight)
        {
            return;
        }

        for (int i = 0; i < text.Length; i++)
        {
            int px = x + i;
            if (px >= 0 && px < BoardWidth)
            {
                _backBuffer[y, px] = new Cell(text[i], color);
            }
        }
    }

    private static void FlushBackBufferToConsole()
    {
        ConsoleColor currentColor = Console.ForegroundColor;

        for (int y = 0; y < BoardHeight; y++)
        {
            int x = 0;
            while (x < BoardWidth)
            {
                Cell back = _backBuffer[y, x];
                Cell front = _frontBuffer[y, x];

                if (!_forceFullRedraw && back.Char == front.Char && back.Color == front.Color)
                {
                    x++;
                    continue;
                }

                int runStart = x;
                ConsoleColor runColor = back.Color;
                StringBuilder sb = new StringBuilder();

                while (x < BoardWidth)
                {
                    Cell nextBack = _backBuffer[y, x];
                    Cell nextFront = _frontBuffer[y, x];
                    bool changed = _forceFullRedraw || nextBack.Char != nextFront.Char || nextBack.Color != nextFront.Color;

                    if (!changed || nextBack.Color != runColor)
                    {
                        break;
                    }

                    sb.Append(nextBack.Char);
                    _frontBuffer[y, x] = nextBack;
                    x++;
                }

                if (sb.Length > 0)
                {
                    try
                    {
                        Console.SetCursorPosition(runStart, y);
                        if (currentColor != runColor)
                        {
                            Console.ForegroundColor = runColor;
                            currentColor = runColor;
                        }
                        Console.Write(sb.ToString());
                    }
                    catch
                    {
                        // 창 크기가 작아 커서 위치 설정에 실패할 경우 안전하게 건너뜀
                    }
                }
            }
        }

        _forceFullRedraw = false;
    }
}

internal static class Program
{
    private static void Main()
    {
        Game.Run();
    }
}