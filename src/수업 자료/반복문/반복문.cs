// 반복문
// 반복 표현식을 줄일 수 있는 방법이 있을까?
// 프로그래머는 반복문을 진짜 싫어한다.

// for 반복문


for (int a = 0; a < 10; a++)
{
    Console.WriteLine(a);
}


// while 반복문
// 무한 반복문
// 게임을 종료할 조건
int grade = 0;
bool gameFinish = grade >= 10;
while (true)
{
    grade++;
    Console.WriteLine($"현재 등급 +{grade}");

    if (gameFinish)
    {
        Console.WriteLine("게임 종료");
        break;
    }
}