메모장.Exe();

// 클래스

// 기초 문법
// 클래스를 만드는 법

// 클래스를 사용하는 법
public class 클래스
{
    // 필드
    private int field;
    // 생성자
    public 클래스(int value)
    {
        field = value;
    }
    // 메서드
    public void DisplayField()
    {
        Console.WriteLine($"Field Value: {field}");
    }
}

public class Enemy
{
    public string name; //몬스터 이름을 문자열로 표현할 수 있다.
    public int hp;      //몬스터 체력을 정수로 표현할 수 있다.
    public float x, y;  //몬스터 위치를 실수로 표현할 수 있다.

    // 위에 정의한 속성으로 무엇을 하는가? - 기능, 멤버 함수,

    // 접근 제한자 
    // public : 모든 곳에서 접근 가능
    // private : 클래스 내부에서만 접근 가능
    // protected : 상속받은 클래스에서 접근 가능
    // internal : 같은 어셈블리 내에서 접근 가능


    public void Hello()
    {
        Console.WriteLine($"안녕하세요. 저는 {name}입니다.");
    }
}

public class 메모장
{
    public static void Exe()
    {
        Enemy 슬라임A = new Enemy();
        Enemy 슬라임B = new Enemy();

        슬라임A.name = "슬라임A";
        슬라임B.name = "슬라임B";

        슬라임A.Hello();
        슬라임B.Hello();


    }
}