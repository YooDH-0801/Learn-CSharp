const int StartGold = 100;
const int StartGrade = 0;
const int StartBestGrade = 0;
const int WinGrade = 10;
const int MinEnhanceCost = 10;

Console.OutputEncoding = System.Text.Encoding.UTF8;

int gold = StartGold;
int grade = StartGrade;
int bestGrade = StartBestGrade;
string message = "최고 등급 +10 무기를 만들어 보세요!";
bool running = true;

Random rng = new Random();

while (running)
{
    // ① Render - 상태줄, 조건줄, 대안줄, 메시지 출력
    int cost = (grade + 1) * 10;
    int chance = 90 - (grade * 8);
    int sellPrice = grade * 20;

    Console.Clear();
    Console.WriteLine("=== 무기 강화 ===");
    Console.WriteLine($"골드: {gold} | 무기: +{grade} | 최고: +{bestGrade}");
    Console.WriteLine($"강화 비용: {cost} | 성공 확률: {chance}%");
    Console.WriteLine($"판매 가격: {sellPrice} | 실패하면 무기는 +0으로 돌아갑니다.");
    Console.WriteLine(message);

    // ② 종료 조건 검사 (승리 / 파산)
    if (grade >= WinGrade)
    {
        Console.WriteLine("+10 달성! 승리했습니다.");
        break;
    }

    if (gold < MinEnhanceCost && grade == 0)
    {
        Console.WriteLine($"파산! 최고 기록: +{bestGrade}");
        break;
    }

    // ③ Input - 메뉴 입력
    Console.WriteLine("[1] 강화 [2] 팔기 [0] 종료");
    Console.Write("> ");
    string? input = Console.ReadLine()?.Trim();

    // ④ Update - 조건문 분기
    switch (input)
    {
        case "1": // 강화 시도
            if (gold < cost)
            {
                message = "강화 비용이 부족합니다. 무기를 팔아 보세요.";
            }
            else
            {
                gold -= cost;
                int roll = rng.Next(0, 100); // 0 ~ 99

                if (roll < chance)
                {
                    grade += 1;
                    if (grade > bestGrade)
                    {
                        bestGrade = grade;
                    }
                    message = $"강화 성공! +{grade}";
                }
                else
                {
                    grade = 0;
                    message = "강화 실패! 새 +0 무기로 다시 시작합니다.";
                }
            }
            break;

        case "2": // 판매
            if (grade == 0)
            {
                message = "+0 무기는 팔 수 없습니다.";
            }
            else
            {
                gold += sellPrice;
                grade = 0;
                message = $"{sellPrice}골드에 판매했습니다. 새 +0 무기를 받았습니다.";
            }
            break;

        case "0": // 종료
            running = false;
            break;

        default: // 잘못된 입력
            message = "1, 2, 0 중 하나를 입력하세요.";
            break;
    }
}