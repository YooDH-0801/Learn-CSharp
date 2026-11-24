## 2026-11-18 — 콘솔 입출력과 실습 3문제, 입력 오류 처리

- 한 일: `02c-콘솔입출력.cs`에 `Console.WriteLine`·`ReadLine`·문자열 보간(`$"..."`)을 정리했고, 실습 3문제(개인정보 입출력·계산기·성인 판별)를 각각 `.cs`로 작성해 실행 확인.
- 막힌 점: 나이 입력에 숫자가 아닌 값을 넣자 `int.Parse(Console.ReadLine())`에서 프로그램이 예외로 멈춤(`FormatException`).
- 시도 → 해결: 처음엔 입력을 믿고 바로 `int.Parse` 했다가 멈춤. `int.TryParse(input, out int age)`로 바꿔, 실패하면 "올바른 숫자를 입력해주세요"를 출력하도록 분기. 입력 검증을 분리하니 멈추지 않음.
- 배운 점: `Console.ReadLine()`은 **항상 string**이다. 숫자로 쓰려면 변환해야 하고, 사용자 입력은 실패를 전제로 `TryParse`로 받는다.