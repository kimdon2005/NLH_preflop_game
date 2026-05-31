# GTO Open Preflop Trainer

An F# console game for practicing first-action preflop decisions in 8-ring Texas Hold'em MTT spots.

The player is shown a stack depth, position, and starting hand. The player chooses one action, then the game immediately shows the stored strategy frequencies and grades the answer.

The game is not a full poker simulator. It focuses on learning mathematically studied preflop opening ranges before the flop.

## Poker Background

Texas Hold'em gives each player two private cards. Five community cards may be dealt in later streets: the flop, turn, and river. Players make the best five-card poker hand from their private cards and the community cards.

The two private cards are the starting hand. Card ranks are ordered from low to high as `2 3 4 5 6 7 8 9 T J Q K A`, so hands with higher cards are often stronger before the flop. For example, `AA` is the strongest starting hand and has very high heads-up equity against any other single starting hand.

Betting happens before the flop, after the flop, after the turn, and after the river. This project only covers the decision before the flop, so there is no postflop play, board dealing, showdown, or chip movement after the first decision.

In poker strategy, GTO means Game Theory Optimal. In this project, GTO is used as a mathematical reference for the quiz answer. It does not mean that the same action is always the best exploitative action against every real opponent.

## Position Guide

The trainer uses 8-ring MTT open-position labels:

- `UTG`: Under the Gun, the earliest open position.
- `UTG1`: the seat immediately after UTG.
- `LJ`: Lojack, a middle position.
- `HJ`: Hijack, the position before cutoff.
- `CO`: Cutoff, the position before button.
- `BTN`: Button, the last position after the flop.
- `SB`: Small Blind, posted blind position before the big blind.

Earlier positions such as UTG generally need tighter ranges because more players remain behind. Later positions such as BTN can usually open wider because fewer players remain and postflop position is better.

## Requirements

- .NET SDK 10.0 or newer
- A terminal that can run `dotnet`

No API keys, database, video files, solver installation, or external poker software are required to run the final game.

This project is implemented in F# and targets .NET 10 through `net10.0` in `HoldemPreflopTrainerOpen.fsproj`.

The project uses the NuGet package `Spectre.Console` for terminal formatting. `dotnet run` or `dotnet build` restores this package automatically.

## How to Run

From this repository directory:

```bash
dotnet run
```

To choose a different number of hands:

```bash
dotnet run -- --hands 20
```

To verify that the included chart data loads correctly:

```bash
dotnet run -- --audit
```

You can also build first and then run:

```bash
dotnet build
dotnet run
```

## Gameplay

Each session presents random preflop quiz hands.

For each hand, the game displays:

- Title: `유효스택`
- Effective stack
- Hero position
- Starting hand
- Rule set: 8-ring MTT ChipEV, no ICM

The available actions are:

- `Allin`
- `Raise`
- `Call`
- `Fold`

Enter `1`, `2`, `3`, or `4`, or type the action name.

If the user enters an invalid action, the game prints an error message and asks again.

After each choice, the game shows:

- The frequency mix for all four actions
- The highest-frequency action
- Whether the player's answer was the best action, an acceptable mixed action, or incorrect
- A short explanation

At the end of the session, the game prints a session review with accuracy and a hand log.

The session review is the end condition of the game. It summarizes all player choices and then the program exits after the final key press.

## Scoring

The game grades each answer using the stored frequency mix:

- `Best Action`: the selected action is the highest-frequency action.
- `Mixed Strategy OK`: the selected action is not the highest-frequency action, but it appears in the stored mix with at least 5% frequency.
- `Incorrect`: the selected action has less than 5% frequency in the stored mix.

The final summary reports both best-action accuracy and broader GTO-compatible accuracy.

## Strategy Data

The final game uses the included file:

```text
gto_open_chart.csv
```

This CSV contains the strategy data used at runtime. It covers:

- Stack depths: `4bb, 8bb, 10bb, 12bb, 15bb, 20bb, 25bb, 30bb, 40bb, 50bb, 80bb, 100bb`
- Positions: `UTG, UTG1, LJ, HJ, CO, BTN, SB`
- All 169 starting hand classes for each stack and position

The game randomly selects from this data. About half of the questions are weighted toward boundary hands where `Fold` and `Raise/Allin` are both strategically relevant.

## Project Files

- `Program.fs`: Main F# game code
- `HoldemPreflopTrainerOpen.fsproj`: F# project file
- `gto_open_chart.csv`: Included runtime strategy data

The repository may also contain build output folders such as `bin/` and `obj/`; these are not required to understand the source code.

## Clean Machine Test

To test from a fresh clone:

```bash
git clone https://github.com/kimdon2005/NLH_preflop_game.git
cd NLH_preflop_game
dotnet --version
dotnet run -- --audit
dotnet run
```

If `dotnet --version` is not available, install the .NET SDK first.

## Troubleshooting

- If package restore fails, run `dotnet restore` and then retry `dotnet run`.
- If the terminal output looks cramped, widen the terminal window.
- If the game does not start because .NET 10 is missing, install the .NET 10 SDK and run the same commands again.

## Implemented Requirements

The final game has the following observable behavior:

1. The game starts as a terminal program with `dotnet run`.
2. The game presents random preflop quiz hands.
3. Each quiz hand shows the effective stack, position, starting hand, and rule set.
4. The user selects one of `Allin`, `Raise`, `Call`, or `Fold`.
5. After the user selects an action, the game prints the stored strategy frequency for each action.
6. The game grades the selected action as `Best Action`, `Mixed Strategy OK`, or `Incorrect`.
7. The game continues for 10 hands by default.
8. The number of hands can be changed with `--hands N`.
9. At the end of the session, the game prints a summary table and hand log.
10. The game can verify its included strategy data with `dotnet run -- --audit`.

## Conformance to Project Instructions

- The implementation is written in F#.
- The project uses .NET 10.
- The repository includes this `README.md`.
- The runtime strategy data file `gto_open_chart.csv` is included in the repository.
- The game can be run from the repository using only the commands in this README.

## Changes from Proposal

The original proposal described a broader preflop trainer with many action-tree situations, including opens, facing opens, 3-bets, 4-bets, blind defense, squeeze spots, and multiway preflop states. The final project narrows the game to first-action open decisions only.

Justification: the final strategy source is a concrete set of stack-by-position open charts. Using this source directly makes the game more consistent and avoids unsupported invented GTO data.

The original proposal listed only `Call`, `Raise`, and `Fold` as player actions. The final game uses `Allin`, `Raise`, `Call`, and `Fold`.

Justification: the final chart data distinguishes all-in actions from normal raises, especially at short stack depths. Keeping `Allin` as a separate action makes the quiz match the data more accurately.

The original proposal expected a simple manually defined GTO strategy table. The final game uses an external CSV strategy table included in the repository.

Justification: the CSV provides a larger and more systematic data set than a small hand-written table, while still keeping the final game easy to run without extra dependencies.

The original implementation plan suggested splitting the code into several files such as `Types.fs`, `ScenarioGenerator.fs`, `StrategyTable.fs`, and `Evaluator.fs`. The final game keeps the source in `Program.fs`.

Justification: the final game is small enough that a single file is simpler for peer reviewers to inspect and run.

The earlier prototype included optional generated explanations through an OpenAI API key. The final submitted game removes that requirement.

Justification: peer review should work from the README instructions alone on a clean machine. Removing the API dependency makes the project more reliable and easier to grade.

## LLM Usage

I used an LLM while developing this project.

What the LLM was used for:

- Help writing and revising the F# console game code.
- Help converting the open-chart data into a CSV-based runtime format.
- Help writing this README and checking it against the project specification.
- Help debugging build errors and improving the text shown to the player.

What had to be manually changed or reprompted:

- I had to refine the game scope after deciding to use first-action open-chart data instead of the broader generated scenario model from the original proposal.
- I had to ask for several wording changes because some generated explanations mentioned internal source files or implementation details that should not appear in the player-facing UI.
- I had to verify the project commands manually with `dotnet build` and `dotnet run -- --audit`.

Main thing the LLM did not do correctly on the first try:

- The LLM could generate code and explanations, but it could not know by itself which player-facing wording would be appropriate for the final game. I reviewed the output and removed implementation-detail text from the UI.

---

# 한국어 README

이 프로젝트는 8링 텍사스 홀덤 MTT 상황에서 첫 프리플랍 액션을 연습하는 F# 콘솔 게임입니다.

텍사스 홀덤의 일부 전략은 게임 이론 관점에서 수학적으로 분석되어 있습니다. 이 프로젝트는 그런 분석 결과를 참고한 전략 데이터를 바탕으로 플레이어의 선택을 평가합니다.

이를 GTO(Game Theory Optimal)라고 부릅니다. 다만 여기서의 GTO는 퀴즈 채점을 위한 수학적 기준이며, 실제 모든 상대를 상대로 항상 가장 좋은 실전 선택이라는 뜻은 아닙니다.

텍사스 홀덤은 각 플레이어가 2장의 개인 카드를 받고, 이후 최대 5장의 커뮤니티 카드가 공개되는 게임입니다. 플레이어는 개인 카드와 커뮤니티 카드를 조합해 가장 강한 5장 족보를 만들고, 쇼다운까지 진행되면 가장 높은 족보를 가진 플레이어가 승리합니다.

처음 배부받는 2장의 카드를 시작 핸드라고 하며, 두 장의 카드 조합으로 프리플랍에서의 상대적인 강함을 추측할 수 있습니다. 카드 랭크는 낮은 순서부터 `2 3 4 5 6 7 8 9 T J Q K A`로 표시합니다. 일반적으로 높은 카드로 구성된 핸드일수록 강하며, `AA`는 가장 강한 시작 핸드입니다. 실제로 `AA`는 다른 어떤 단일 시작 핸드를 상대로도 일대일에서 매우 높은 승률을 가집니다.

처음 공개되는 3장의 공유 카드를 플랍, 그 다음 한 장을 턴, 마지막 한 장을 리버라고 합니다.

베팅은 플랍 전, 플랍 후, 턴 후, 리버 후에 진행됩니다. 리버 액션이 폴드로 끝나지 않으면 자신의 패를 공개해 승부를 결정합니다.

이 게임은 그중 플랍 전, 즉 프리플랍에서 포지션에 따른 오픈 공격 레인지를 학습하는 것을 목표로 합니다.

각 포지션 설명은 아래와 같습니다.

- `UTG`: Under the Gun. 가장 먼저 오픈 액션을 하는 자리입니다.
- `UTG1`: UTG 바로 다음 자리입니다.
- `LJ`: Lojack. 중간 포지션에 해당합니다.
- `HJ`: Hijack. 컷오프 바로 앞 자리입니다.
- `CO`: Cutoff. 버튼 바로 앞 자리입니다.
- `BTN`: Button. 플랍 이후 가장 늦게 행동할 수 있는 유리한 포지션입니다.
- `SB`: Small Blind. 빅블라인드 바로 앞에 있는 블라인드 포지션입니다.

간략히 말하면 UTG에 가까운 앞 포지션일수록 뒤에 남은 플레이어가 많기 때문에 더 좋은 핸드를 사용해야 합니다. 반대로 BTN처럼 뒤 포지션일수록 남은 플레이어가 적고 플랍 이후 포지션도 좋아 더 넓은 레인지로 오픈할 수 있습니다.

이 게임은 어떤 핸드로 공격하고 어떤 핸드는 폴드해야 하는 것이 GTO적으로 올바른지, 반복적으로 연습하기 위해 만들었습니다.

플레이어는 유효 스택, 포지션, 시작 핸드를 보고 하나의 액션을 선택합니다. 선택 후 게임은 저장된 전략 빈도를 보여 주고, 플레이어의 선택을 평가합니다.

최종적으로 플레이어의 모든 선택을 평가하는 표를 보여 주고 게임을 마무리합니다.

## 요구사항

- .NET SDK 10.0 이상
- `dotnet` 명령을 실행할 수 있는 터미널

최종 게임을 실행하는 데 API 키, 데이터베이스, 영상 파일, 솔버 설치, 외부 포커 프로그램은 필요하지 않습니다.

이 프로젝트는 F#으로 작성되었고, `HoldemPreflopTrainerOpen.fsproj`에서 `net10.0`을 대상으로 합니다.

터미널 출력 형식을 위해 NuGet 패키지 `Spectre.Console`을 사용합니다. `dotnet run` 또는 `dotnet build`를 실행하면 이 패키지는 자동으로 복원됩니다.

## 실행 방법

이 저장소 디렉터리에서 실행합니다.

```bash
dotnet run
```

문제 수를 바꾸고 싶으면 다음처럼 실행합니다.

```bash
dotnet run -- --hands 20
```

포함된 차트 데이터가 정상적으로 로드되는지 확인하려면 다음 명령을 실행합니다.

```bash
dotnet run -- --audit
```

먼저 빌드한 뒤 실행할 수도 있습니다.

```bash
dotnet build
dotnet run
```

## 게임 방식

각 세션은 랜덤 프리플랍 퀴즈들로 구성됩니다.

각 문제에서 게임은 다음 정보를 보여 줍니다.

- 제목: `유효스택`
- 유효 스택
- 히어로 포지션
- 시작 핸드
- 규칙: 8링 MTT ChipEV, ICM 미반영

선택 가능한 액션은 다음 네 가지입니다.

- `Allin`
- `Raise`
- `Call`
- `Fold`

입력은 `1`, `2`, `3`, `4`로 해도 되고, 액션 이름을 직접 입력해도 됩니다.

잘못된 액션을 입력하면 게임은 오류 메시지를 출력하고 다시 입력을 요청합니다.

선택 후 게임은 다음 정보를 보여 줍니다.

- 네 액션의 전략 빈도
- 가장 높은 빈도의 액션
- 플레이어의 선택이 최빈 액션인지, 허용 가능한 믹스인지, 틀린 선택인지
- 짧은 설명

세션이 끝나면 정확도와 핸드 로그가 포함된 요약 결과를 출력합니다.

이 세션 요약이 게임의 종료 조건입니다. 최종 표를 확인한 뒤 마지막 키를 누르면 프로그램이 종료됩니다.

## 채점 방식

게임은 저장된 전략 빈도를 기준으로 각 선택을 평가합니다.

- `Best Action`: 선택한 액션이 가장 높은 빈도의 액션입니다.
- `Mixed Strategy OK`: 선택한 액션이 최빈 액션은 아니지만, 저장된 전략 믹스에서 5% 이상 등장합니다.
- `Incorrect`: 선택한 액션이 저장된 전략 믹스에서 5% 미만입니다.

최종 요약은 최빈 액션 기준 정확도와 더 넓은 GTO 호환 선택 비율을 함께 보여 줍니다.

## 전략 데이터

최종 게임은 저장소에 포함된 다음 파일을 런타임 데이터로 사용합니다.

```text
gto_open_chart.csv
```

이 CSV 파일은 게임에서 사용하는 전략 데이터를 담고 있습니다. 포함 범위는 다음과 같습니다.

- 스택: `4bb, 8bb, 10bb, 12bb, 15bb, 20bb, 25bb, 30bb, 40bb, 50bb, 80bb, 100bb`
- 포지션: `UTG, UTG1, LJ, HJ, CO, BTN, SB`
- 각 스택과 포지션마다 169개 시작 핸드 클래스

게임은 이 데이터에서 문제를 랜덤으로 선택합니다. 문제의 약 절반은 `Fold`와 `Raise/Allin`이 모두 의미 있는 경계 핸드에서 가중 출제됩니다.

## 프로젝트 파일

- `Program.fs`: 메인 F# 게임 코드
- `HoldemPreflopTrainerOpen.fsproj`: F# 프로젝트 파일
- `gto_open_chart.csv`: 런타임 전략 데이터

저장소에 `bin/`, `obj/` 같은 빌드 출력 폴더가 있을 수 있지만, 소스 코드를 이해하거나 실행하는 데 필수는 아닙니다.

## 깨끗한 환경에서 테스트하는 방법

새로 clone한 뒤 다음 순서로 테스트할 수 있습니다.

```bash
git clone https://github.com/kimdon2005/NLH_preflop_game.git
cd NLH_preflop_game
dotnet --version
dotnet run -- --audit
dotnet run
```

`dotnet --version` 명령이 동작하지 않으면 먼저 .NET SDK를 설치해야 합니다.

## 문제 해결

- 패키지 복원이 실패하면 `dotnet restore`를 실행한 뒤 `dotnet run`을 다시 시도합니다.
- 터미널 화면이 좁게 보이면 터미널 창의 너비를 넓힙니다.
- .NET 10이 없어서 게임이 시작되지 않으면 .NET 10 SDK를 설치한 뒤 같은 명령을 다시 실행합니다.

## 구현된 요구사항

최종 게임의 관찰 가능한 동작은 다음과 같습니다.

1. `dotnet run`으로 터미널 게임이 시작됩니다.
2. 게임은 랜덤 프리플랍 퀴즈를 제시합니다.
3. 각 퀴즈는 유효 스택, 포지션, 시작 핸드, 규칙을 보여 줍니다.
4. 사용자는 `Allin`, `Raise`, `Call`, `Fold` 중 하나를 선택합니다.
5. 사용자가 액션을 선택하면 게임은 각 액션의 저장된 전략 빈도를 출력합니다.
6. 게임은 선택을 `Best Action`, `Mixed Strategy OK`, `Incorrect`로 평가합니다.
7. 기본 세션은 10핸드로 진행됩니다.
8. `--hands N` 옵션으로 핸드 수를 변경할 수 있습니다.
9. 세션 종료 시 요약 테이블과 핸드 로그를 출력합니다.
10. `dotnet run -- --audit` 명령으로 포함된 전략 데이터를 검증할 수 있습니다.

## 과제 지시사항 준수

- 구현은 F#으로 작성되었습니다.
- 프로젝트는 .NET 10을 사용합니다.
- 저장소에는 이 `README.md`가 포함되어 있습니다.
- 런타임 전략 데이터 파일 `gto_open_chart.csv`가 저장소에 포함되어 있습니다.
- 이 README에 적힌 명령만으로 저장소에서 게임을 실행할 수 있습니다.

## 제안서 대비 변경사항

원래 제안서는 오픈, 오픈 대응, 3-bet, 4-bet, 블라인드 디펜스, 스퀴즈, 멀티웨이 프리플랍 상황 등 더 넓은 액션 트리를 다루는 트레이너를 설명했습니다. 최종 프로젝트는 첫 오픈 액션 결정만 다루도록 범위를 좁혔습니다.

이유: 최종 전략 소스가 스택별/포지션별 오픈 차트 데이터이기 때문입니다. 이 데이터를 직접 사용하는 편이 더 일관적이고, 근거 없는 임의 GTO 데이터를 만드는 것보다 안전합니다.

원래 제안서의 플레이어 액션은 `Call`, `Raise`, `Fold` 세 가지였습니다. 최종 게임은 `Allin`, `Raise`, `Call`, `Fold` 네 가지를 사용합니다.

이유: 최종 차트 데이터는 특히 짧은 스택에서 올인과 일반 레이즈를 구분합니다. `Allin`을 별도 액션으로 유지해야 퀴즈가 데이터와 더 정확히 일치합니다.

원래 제안서는 간단한 수동 GTO 전략 테이블을 예상했습니다. 최종 게임은 저장소에 포함된 외부 CSV 전략 테이블을 사용합니다.

이유: CSV는 작은 하드코딩 테이블보다 더 크고 체계적인 데이터를 제공합니다. 동시에 추가 의존성 없이 쉽게 실행할 수 있습니다.

원래 구현 계획은 `Types.fs`, `ScenarioGenerator.fs`, `StrategyTable.fs`, `Evaluator.fs` 등 여러 파일로 나누는 구조였습니다. 최종 게임은 `Program.fs` 한 파일에 소스 코드를 유지합니다.

이유: 최종 게임 규모가 작기 때문에 한 파일이 동료 평가자가 읽고 실행하기에 더 단순합니다.

## LLM 사용

이 프로젝트를 개발하는 과정에서 LLM을 사용했습니다.

LLM을 사용한 부분:

- F# 콘솔 게임 코드 작성과 수정 보조
- 오픈 차트 데이터를 CSV 기반 런타임 형식으로 바꾸는 작업 보조
- README 작성과 과제 명세 검토 보조
- 빌드 오류 디버깅과 플레이어에게 보이는 문구 개선

직접 수정하거나 다시 요청해야 했던 부분:

- 원래 제안서의 넓은 시나리오 생성 모델 대신 첫 액션 오픈 차트 데이터를 사용하는 방향으로 게임 범위를 조정해야 했습니다.
- 일부 생성된 설명이 내부 파일명이나 구현 세부사항을 플레이어 화면에 보여 주었기 때문에, 사용자에게 보이지 않도록 문구 수정을 여러 번 요청했습니다.
- `dotnet build`와 `dotnet run -- --audit` 명령이 실제로 동작하는지 직접 검증했습니다.

LLM이 처음부터 정확히 하지 못한 핵심 부분:

- LLM은 코드와 설명을 생성할 수 있었지만, 최종 게임에서 플레이어에게 어떤 문구가 적절한지 스스로 정확히 판단하지는 못했습니다. 그래서 출력 문구를 검토하고 구현 세부사항을 UI에서 제거했습니다.
