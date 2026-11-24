Player hero = new Player("용사", 100); // 객체 생성
// hero.Name = "용사"; // 이름 설정
// hero.Hp = 100; // 체력 설정
hero.TakeDamage(30);

Player slime = new Player("슬라임", 30);
// slime.Name = "슬라임"; // 이름 설정
// slime.Hp = 30; // 체력 설정
slime.TakeDamage(30);

hero.PrintStatus();
slime.PrintStatus();

class Player
{
    public string Name = "";
    public int Hp = 100;

    public Player(string name, int hp)
    {
        this.Name = name;
        this.Hp = hp;
    }

    public void TakeDamage(int amount)
    {
        Hp -= amount;
    }

    public void PrintStatus()
    {
        Console.WriteLine($"{Name}: HP {Hp}");
    }
}