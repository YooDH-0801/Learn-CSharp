// 1. 똥피하기 게임 (기획)
//      콘솔 화면 위에서 똥인 '*'가 떨어진다
//      가끔씩 2x2 큰 똥도 떨어진다
//      플레이어는 방향키(좌, 우)를 입력 받아 움직일 수 있다
//
//
//
//
// 2. 시스템 설계
// (1) 게임 루프를 반드시 포함시켜야 함
// (2) 게임 루프는 입력, 갱신, 렌더링 순서로 구성되어야 함
// (3) 입력은 이벤트로 구현
// (4) 갱신은 delta time을 고려하여 구현
// (5) 렌더링은 화면에 그리는 것을 의미 
// (6) 언어는 C#을 사용
// (7) 


// 부록. 최상문을 Main코드를 간략하게 실행할 수 있는 구조로 만드세요. 예를 들어, Game.Run() 같은 형태여야 함
// Game 클래스 시스템 설계를 충실히 구성하기 위해 여러 개의 클래스를 구성해야 함

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

// [부록] 최상위 문: Game.Run() 형태로 간략하게 실행
var game = new Game(width: 40, height: 25);
game.Run();

#region 1. Core Game Loop & Manager
public class Game
{
    public const int TargetFPS = 60;
    public readonly int Width;
    public readonly int Height;

    private bool _isRunning;
    private bool _isGameOver;
    private float _score;

    private readonly InputHandler _inputHandler;
    private readonly Renderer _renderer;
    private readonly Player _player;
    private readonly List<Poop> _poops;
    private readonly Random _random;

    private float _spawnTimer;
    private float _spawnInterval = 0.25f; // 0.25초마다 똥 생성

    public Game(int width, int height)
    {
        Width = width;
        Height = height;

        _inputHandler = new InputHandler();
        _renderer = new Renderer(width, height);
        _player = new Player(startX: width / 2, y: height - 2, maxWidth: width);
        _poops = new List<Poop>();
        _random = new Random();

        // (3) 입력 이벤트 구독
        _inputHandler.OnKeyPressed += HandleInput;
    }

    public void Run()
    {
        Console.CursorVisible = false;
        _isRunning = true;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        double lastTime = stopwatch.Elapsed.TotalSeconds;

        // (1) 게임 루프
        while (_isRunning)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentTime - lastTime);
            lastTime = currentTime;

            // (2) 입력 -> 갱신 -> 렌더링 순서 준수
            ProcessInput();
            Update(deltaTime);
            Render();

            // CPU 점유율 방어 및 프레임 페이싱
            int sleepTime = (int)((1000.0 / TargetFPS) - (stopwatch.Elapsed.TotalSeconds - currentTime) * 1000);
            if (sleepTime > 0)
            {
                Thread.Sleep(sleepTime);
            }
        }
    }

    private void ProcessInput()
    {
        _inputHandler.PollEvents();
    }

    private void HandleInput(ConsoleKey key)
    {
        if (_isGameOver)
        {
            if (key == ConsoleKey.R) ResetGame();
            else if (key == ConsoleKey.Escape) _isRunning = false;
            return;
        }

        switch (key)
        {
            case ConsoleKey.LeftArrow:
                _player.Move(-1);
                break;
            case ConsoleKey.RightArrow:
                _player.Move(1);
                break;
            case ConsoleKey.Escape:
                _isRunning = false;
                break;
        }
    }

    // (4) delta time을 고려한 갱신
    private void Update(float deltaTime)
    {
        if (_isGameOver) return;

        _score += deltaTime * 10f; // 생존 시간에 비례해 점수 증가

        // 난이도 점진적 상승 (생성 주기 단축)
        _spawnInterval = Math.Max(0.08f, 0.25f - (_score * 0.0005f));

        // 똥 스폰 로직
        _spawnTimer += deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0f;
            SpawnPoop();
        }

        // 모든 똥 낙하 및 충돌 검사
        for (int i = _poops.Count - 1; i >= 0; i--)
        {
            Poop poop = _poops[i];
            poop.Update(deltaTime);

            // AABB 충돌 판정
            if (poop.CheckCollision(_player.X, _player.Y))
            {
                _isGameOver = true;
                break;
            }

            // 화면 바닥을 벗어나면 제거
            if (poop.Y >= Height)
            {
                _poops.RemoveAt(i);
            }
        }
    }

    private void SpawnPoop()
    {
        // 20% 확률로 2x2 큰 똥 생성, 80% 확률로 1x1 일반 똥 생성
        bool isBig = _random.NextDouble() < 0.2;
        int size = isBig ? 2 : 1;
        int spawnX = _random.Next(0, Width - size + 1);
        float speed = isBig ? _random.Next(10, 16) : _random.Next(14, 24); // 큰 똥은 약간 묵직하게 낙하

        _poops.Add(new Poop(spawnX, 0, size, speed));
    }

    // (5) 화면 렌더링
    private void Render()
    {
        _renderer.ClearBuffer();

        // 플레이어 그리기
        _renderer.DrawChar(_player.X, _player.Y, '▲');

        // 똥 그리기 (1x1 또는 2x2)
        foreach (var poop in _poops)
        {
            int renderY = (int)Math.Floor(poop.Y);
            for (int dy = 0; dy < poop.Size; dy++)
            {
                for (int dx = 0; dx < poop.Size; dx++)
                {
                    _renderer.DrawChar(poop.X + dx, renderY + dy, '*');
                }
            }
        }

        _renderer.Present((int)_score, _isGameOver);
    }

    private void ResetGame()
    {
        _poops.Clear();
        _player.Reset(Width / 2);
        _score = 0;
        _spawnTimer = 0;
        _isGameOver = false;
    }
}
#endregion

#region 2. Event-Driven Input Handler
public class InputHandler
{
    // 키 입력 발생 시 구독자들에게 알리는 이벤트
    public event Action<ConsoleKey>? OnKeyPressed;

    public void PollEvents()
    {
        // 버퍼에 쌓인 키 입력을 모두 소비하여 입력 지연 방지
        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            OnKeyPressed?.Invoke(keyInfo.Key);
        }
    }
}
#endregion

#region 3. Game Entities (Player & Poop)
public class Player
{
    public int X { get; private set; }
    public int Y { get; private set; }
    private readonly int _maxWidth;

    public Player(int startX, int y, int maxWidth)
    {
        X = startX;
        Y = y;
        _maxWidth = maxWidth;
    }

    public void Move(int direction)
    {
        X = Math.Clamp(X + direction, 0, _maxWidth - 1);
    }

    public void Reset(int startX)
    {
        X = startX;
    }
}

public class Poop
{
    public int X { get; }
    public float Y { get; private set; } // deltaTime 기반 부드러운 낙하를 위해 float 사용
    public int Size { get; }             // 1이면 1x1, 2면 2x2
    public float Speed { get; }

    public Poop(int x, float y, int size, float speed)
    {
        X = x;
        Y = y;
        Size = size;
        Speed = speed;
    }

    public void Update(float deltaTime)
    {
        Y += Speed * deltaTime;
    }

    public bool CheckCollision(int targetX, int targetY)
    {
        int currentY = (int)Math.Floor(Y);
        return targetX >= X && targetX < X + Size &&
               targetY >= currentY && targetY < currentY + Size;
    }
}
#endregion

#region 4. Double-Buffered Console Renderer
public class Renderer
{
    private readonly int _width;
    private readonly int _height;
    private readonly char[,] _buffer;
    private readonly StringBuilder _frameBuilder;

    public Renderer(int width, int height)
    {
        _width = width;
        _height = height;
        _buffer = new char[height, width];
        _frameBuilder = new StringBuilder((width + 3) * (height + 5));
    }

    public void ClearBuffer()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _buffer[y, x] = ' ';
            }
        }
    }

    public void DrawChar(int x, int y, char symbol)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            _buffer[y, x] = symbol;
        }
    }

    public void Present(int score, bool isGameOver)
    {
        _frameBuilder.Clear();

        // 상단 UI 및 테두리
        _frameBuilder.AppendLine($" SCORE: {score:D5}   [←/→: 이동, ESC: 종료]");
        _frameBuilder.AppendLine("┌" + new string('─', _width) + "┐");

        // 게임 월드 버퍼 출력
        for (int y = 0; y < _height; y++)
        {
            _frameBuilder.Append('│');
            for (int x = 0; x < _width; x++)
            {
                _frameBuilder.Append(_buffer[y, x]);
            }
            _frameBuilder.AppendLine("│");
        }

        // 하단 테두리
        _frameBuilder.AppendLine("└" + new string('─', _width) + "┘");

        if (isGameOver)
        {
            _frameBuilder.AppendLine("        === GAME OVER ===");
            _frameBuilder.AppendLine("   다시 시작: [R] / 종료: [ESC]");
        }
        else
        {
            // 잔상 제거용 공백 라인
            _frameBuilder.AppendLine("                         ");
            _frameBuilder.AppendLine("                                 ");
        }

        // Console.Clear() 대신 커서를 (0, 0)으로 옮겨 덮어쓰기 (깜빡임 방지)
        Console.SetCursorPosition(0, 0);
        Console.Write(_frameBuilder.ToString());
    }
}
#endregion