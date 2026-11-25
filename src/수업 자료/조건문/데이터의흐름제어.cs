// 분기문
// 코드를 원하는 부분만 실행하겠다.

/*
 *  if(조건문)
 *  {
 *  
 *  }
 * 
 */

// 무엇이 조건이냐? True or False

// 보유 돈, 강화 비용

using note;

int wallet = 10000;
int cost = 500;

bool condition = wallet >= cost;
if (condition)
{
    Console.WriteLine("강화 성공!");
}
else
{
    Console.WriteLine("강화 실패!");
}

// 상수는 변하지 않게 강제한다.
//const int SCISSORS = 1;
//const int ROCK = 2;
//const int PAPER = 3;
//const int CHEAT = 4;

// 1. 가위, 2. 바위, 3. 보, 4. 치트
int choice = int.Parse(Console.ReadLine() ?? "0");

int random = new Random().Next(1, 3);
Console.WriteLine($"컴퓨터는 {random}를 냈습니다.");

switch(choice)
{
    case (int) Choice.Scissors:
        Console.WriteLine("플레이어는 가위를 냈습니다.");
        break;
    case (int) Choice.Rock:
        Console.WriteLine("플레이어는 바위를 냈습니다.");
        break;
    case (int) Choice.Paper:
        Console.WriteLine("플레이어는 보를 냈습니다.");
        break;
    case (int) Choice.Cheat:
        Console.WriteLine("플레이어는 치트를 사용했습니다.");
        break;
    default:
        Console.WriteLine("잘못된 입력입니다.");
        break;
}

/*
if (choice == SCISSORS)
{
    Console.WriteLine("가위를 냈습니다.");
}
else if (choice == ROCK)
{
    Console.WriteLine("바위를 냈습니다.");
}
else if (choice == PAPER)
{
    Console.WriteLine("보를 냈습니다.");
}
else if (choice == CHEAT)
{
    Console.WriteLine("치트를 사용했습니다.");
}
else
{
    Console.WriteLine("잘못된 입력입니다.");
}
*/

if (choice == (int) Choice.Scissors)
{
    switch (random)
    {
        case (int) AttackStyle.Scissors:
            Console.WriteLine("비겼습니다.");
            break;
        case (int) AttackStyle.Rock:
            Console.WriteLine("졌습니다.");
            break;
        case (int) AttackStyle.Paper:
            Console.WriteLine("이겼습니다.");
            break;
        default:
            Console.WriteLine("잘못된 입력입니다.");
            break;
    }
}
else if (choice == (int)Choice.Rock)
{
    switch (random)
    {
        case (int)AttackStyle.Scissors:
            Console.WriteLine("이겼습니다.");
            break;
        case (int)AttackStyle.Rock:
            Console.WriteLine("비겼습니다.");
            break;
        case (int)AttackStyle.Paper:
            Console.WriteLine("졌습니다.");
            break;
        default:
            Console.WriteLine("잘못된 입력입니다.");
            break;
    }
}
else if (choice == (int)Choice.Paper)
{
    switch (random)
    {
        case (int)AttackStyle.Scissors:
            Console.WriteLine("졌습니다.");
            break;
        case (int)AttackStyle.Rock:
            Console.WriteLine("이겼습니다.");
            break;
        case (int)AttackStyle.Paper:
            Console.WriteLine("비겼습니다.");
            break;
        default:
            Console.WriteLine("잘못된 입력입니다.");
            break;
    }
}
else if (choice == (int)Choice.Cheat)
{
    switch (random)
    {
        case (int)AttackStyle.Scissors:
            Console.WriteLine("이겼습니다.");
            break;
        case (int)AttackStyle.Rock:
            Console.WriteLine("이겼습니다.");
            break;
        case (int)AttackStyle.Paper:
            Console.WriteLine("이겼습니다.");
            break;
        default:
            Console.WriteLine("잘못된 입력입니다.");
            break;
    }
}
else
{
    Console.WriteLine("잘못된 입력입니다.");
}