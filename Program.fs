open System
open System.IO
open Spectre.Console

type Action =
    | Fold
    | Call
    | Raise
    | Allin

type ChartEntry =
    { Stack: int
      Position: string
      Hand: string
      AllinPct: int
      RaisePct: int
      CallPct: int
      FoldPct: int
      IsBoundary: bool
      SourceFrame: string }

type ResultGrade =
    | BestAction
    | MixedStrategyOkay
    | Incorrect

type Result =
    { HandNo: int
      Entry: ChartEntry
      Chosen: Action
      Grade: ResultGrade }

let rng = Random()

let actionToText action =
    match action with
    | Fold -> "Fold"
    | Call -> "Call"
    | Raise -> "Raise"
    | Allin -> "Allin"

let gradeToText grade =
    match grade with
    | BestAction -> "Best Action"
    | MixedStrategyOkay -> "Mixed Strategy OK"
    | Incorrect -> "Incorrect"

let actionFrequency action entry =
    match action with
    | Fold -> entry.FoldPct
    | Call -> entry.CallPct
    | Raise -> entry.RaisePct
    | Allin -> entry.AllinPct

let dominantAction entry =
    [ Allin, entry.AllinPct
      Raise, entry.RaisePct
      Call, entry.CallPct
      Fold, entry.FoldPct ]
    |> List.maxBy snd
    |> fst

let evaluateChoice chosen entry =
    let chosenFreq = actionFrequency chosen entry
    if chosen = dominantAction entry then
        BestAction
    elif chosenFreq >= 5 then
        MixedStrategyOkay
    else
        Incorrect

let formatMix entry =
    sprintf
        "Allin %d%% / Raise %d%% / Call %d%% / Fold %d%%"
        entry.AllinPct
        entry.RaisePct
        entry.CallPct
        entry.FoldPct

let csvPath () =
    let local = Path.Combine(AppContext.BaseDirectory, "gto_open_chart.csv")
    if File.Exists local then
        local
    else
        Path.Combine(Directory.GetCurrentDirectory(), "gto_open_chart.csv")

let loadChart () =
    let path = csvPath ()
    if not (File.Exists path) then
        failwithf "Cannot find gto_open_chart.csv at %s" path

    File.ReadLines(path)
    |> Seq.skip 1
    |> Seq.map (fun line ->
        let columns = line.Split(',')
        { Stack = Int32.Parse(columns.[0])
          Position = columns.[1]
          Hand = columns.[2]
          AllinPct = Int32.Parse(columns.[3])
          RaisePct = Int32.Parse(columns.[4])
          CallPct = Int32.Parse(columns.[5])
          FoldPct = Int32.Parse(columns.[6])
          IsBoundary = columns.[7].Equals("true", StringComparison.OrdinalIgnoreCase)
          SourceFrame = columns.[8] })
    |> Seq.toList

let chooseEntry (chart: ChartEntry list) =
    let boundaryPool = chart |> List.filter (fun entry -> entry.IsBoundary)
    let pool =
        if boundaryPool.Length > 0 && rng.NextDouble() < 0.50 then
            boundaryPool
        else
            chart

    pool.[rng.Next(pool.Length)]

let parseHandCount args =
    let rec loop remaining =
        match remaining with
        | "--hands" :: value :: _ ->
            match Int32.TryParse value with
            | true, n when n > 0 -> n
            | _ -> 10
        | _ :: tail -> loop tail
        | [] -> 10

    loop (Array.toList args)

let waitForContinue () =
    if Console.IsInputRedirected then
        Console.ReadLine() |> ignore
    else
        Console.ReadKey(true) |> ignore

let runAudit (chart: ChartEntry list) =
    let stacks =
        chart
        |> List.map (fun entry -> entry.Stack)
        |> List.distinct
        |> List.sort

    let positions =
        chart
        |> List.map (fun entry -> entry.Position)
        |> List.distinct

    let boundaryCount = chart |> List.filter (fun entry -> entry.IsBoundary) |> List.length
    printfn "Chart rows: %d" chart.Length
    printfn "Stacks: %s" (stacks |> List.map (sprintf "%dbb") |> String.concat ", ")
    printfn "Positions: %s" (positions |> String.concat ", ")
    printfn "Boundary rows: %d" boundaryCount
    printfn "Expected rows for 12 stacks * 7 positions * 169 hands: %d" (12 * 7 * 169)

let rec askPlayerAction () =
    AnsiConsole.MarkupLine("[yellow]첫 액션을 선택하세요[/]")
    AnsiConsole.MarkupLine("1. Allin")
    AnsiConsole.MarkupLine("2. Raise")
    AnsiConsole.MarkupLine("3. Call")
    AnsiConsole.MarkupLine("4. Fold")
    AnsiConsole.Markup("[grey]> [/]")

    match Console.ReadLine() with
    | null -> Fold
    | value ->
        match value.Trim().ToLowerInvariant() with
        | "1"
        | "allin"
        | "all-in"
        | "jam" -> Allin
        | "2"
        | "raise"
        | "r" -> Raise
        | "3"
        | "call"
        | "c" -> Call
        | "4"
        | "fold"
        | "f" -> Fold
        | _ ->
            AnsiConsole.MarkupLine("[red]Invalid action. Enter 1, 2, 3, 4, or an action name.[/]")
            askPlayerAction ()

let safeClear () =
    if not Console.IsInputRedirected then
        AnsiConsole.Clear()

let renderWelcome (chart: ChartEntry list) =
    safeClear ()
    AnsiConsole.Write(FigletText("GTO Open").Color(Color.Green))

    let stacks =
        chart
        |> List.map (fun entry -> entry.Stack)
        |> List.distinct
        |> List.sort
        |> List.map (sprintf "%dbb")
        |> String.concat ", "

    let boundaryCount = chart |> List.filter (fun entry -> entry.IsBoundary) |> List.length

    let intro =
        $"""
8-ring MTT ChipEV 첫 오픈 액션 연습입니다.

- 사용 스택: {stacks}
- 포지션: UTG, UTG1, LJ, HJ, CO, BTN, SB
- ICM 미반영
- 문제의 약 50%%는 Fold와 Raise/Allin 경계 핸드에서 가중 출제
- 저장된 경계 핸드 수: {boundaryCount}
"""

    AnsiConsole.Write(
        Panel(intro.Trim())
            .Header("[yellow]유효스택 기반 GTO Open Trainer[/]")
            .Border(BoxBorder.Rounded))

    AnsiConsole.MarkupLine("\n[grey]Press any key to start...[/]")
    waitForContinue ()

let renderScenario handNo totalHands entry =
    safeClear ()
    AnsiConsole.Write(Rule(sprintf "Hand %d / %d" handNo totalHands).RuleStyle("green").Centered())

    let info =
        Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Field")
            .AddColumn("Value")

    info.AddRow("Title", "유효스택") |> ignore
    info.AddRow("Effective Stack", sprintf "%dbb" entry.Stack) |> ignore
    info.AddRow("Position", entry.Position) |> ignore
    info.AddRow("Hand", sprintf "[bold aqua]%s[/]" entry.Hand) |> ignore
    info.AddRow("Rule Set", "8-ring MTT ChipEV, ICM 미반영") |> ignore

    AnsiConsole.Write(info)
    AnsiConsole.WriteLine()
    AnsiConsole.MarkupLine("[grey]정답 빈도와 설명은 선택 후 공개됩니다.[/]")
    AnsiConsole.WriteLine()

let buildExplanation entry choice =
    let chosenFreq = actionFrequency choice entry
    let dominant = dominantAction entry
    let aggressive = entry.AllinPct + entry.RaisePct
    let pureAction =
        [ Allin, entry.AllinPct
          Raise, entry.RaisePct
          Call, entry.CallPct
          Fold, entry.FoldPct ]
        |> List.tryFind (fun (_, frequency) -> frequency = 100)

    let resultText =
        match pureAction, evaluateChoice choice entry with
        | Some(action, _), BestAction ->
            sprintf "%s 100%% 노드이므로 이 선택은 적절하다." (actionToText action)
        | Some(action, _), _ ->
            sprintf "%s 100%% 노드라서 여기서는 %s 선택이 적절하다." (actionToText action) (actionToText action)
        | None, BestAction ->
            "이번 선택은 이 노드에서 가장 높은 빈도의 주전략과 일치한다."
        | None, MixedStrategyOkay ->
            "이번 선택은 최빈 액션은 아니지만, 전략 믹스 안에 포함된다."
        | None, Incorrect ->
            "이번 선택은 이 전략 믹스에서 거의 사용되지 않는 액션이다."

    let optionalTexts =
        [ if entry.IsBoundary then
              "이 핸드는 Fold와 Raise/Allin 쪽 EV가 가까운 경계 구간이다."
          if dominant = Allin && entry.Stack <= 12 then
              "올인이 가장 높은 빈도인 얕은 스택 노드라서 올인 압박과 폴드 에퀴티가 판단의 핵심이다." ]

    String.concat
        "\n\n"
        ([ sprintf "%dbb %s %s의 전략 빈도는 Allin %d%% / Raise %d%% / Call %d%% / Fold %d%% 이며, 가장 높은 빈도는 %s 이다." entry.Stack entry.Position entry.Hand entry.AllinPct entry.RaisePct entry.CallPct entry.FoldPct (actionToText dominant)
           sprintf "당신이 선택한 %s 는 이 노드에서 %d%% 빈도를 가진다." (actionToText choice) chosenFreq
           sprintf "Fold는 %d%%, Raise/Allin 합산 공격 빈도는 %d%%다." entry.FoldPct aggressive ]
         @ optionalTexts
         @ [ resultText ])

let renderImmediateFeedback entry choice =
    let grade = evaluateChoice choice entry
    let header, color =
        match grade with
        | BestAction -> "Best Action", Color.Green
        | MixedStrategyOkay -> "Mixed Strategy OK", Color.Yellow
        | Incorrect -> "Incorrect", Color.Red

    let frequencyTable =
        Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Action")
            .AddColumn("Frequency")

    frequencyTable.AddRow("Allin", sprintf "%d%%" entry.AllinPct) |> ignore
    frequencyTable.AddRow("Raise", sprintf "%d%%" entry.RaisePct) |> ignore
    frequencyTable.AddRow("Call", sprintf "%d%%" entry.CallPct) |> ignore
    frequencyTable.AddRow("Fold", sprintf "%d%%" entry.FoldPct) |> ignore

    let summary =
        sprintf
            "Your action: %s\nAction frequency: %d%%\nBest-frequency action: %s\nGrade: %s"
            (actionToText choice)
            (actionFrequency choice entry)
            (actionToText (dominantAction entry))
            (gradeToText grade)

    AnsiConsole.Write(
        Panel(summary)
            .Header(sprintf "[white]%s[/]" header)
            .Border(BoxBorder.Rounded)
            .BorderColor(color))

    AnsiConsole.WriteLine()
    AnsiConsole.MarkupLine("[bold]GTO Frequency Mix[/]")
    AnsiConsole.Write(frequencyTable)
    AnsiConsole.WriteLine()
    AnsiConsole.Write(
        Panel(buildExplanation entry choice)
            .Header("[cyan]GTO Logic Explanation[/]")
            .Border(BoxBorder.Rounded))

    AnsiConsole.MarkupLine("\n[grey]Press any key for next hand...[/]")
    waitForContinue ()
    grade

let summarizeResults (results: Result list) =
    let total = results.Length
    let exact = results |> List.filter (fun r -> r.Grade = BestAction) |> List.length
    let compatible = results |> List.filter (fun r -> r.Grade <> Incorrect) |> List.length
    let boundaryHands = results |> List.filter (fun r -> r.Entry.IsBoundary) |> List.length

    safeClear ()
    AnsiConsole.Write(Rule("Session Review").RuleStyle("blue").Centered())

    let summary =
        Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value")

    summary.AddRow("Hands Played", string total) |> ignore
    summary.AddRow("Best-Action Hits", sprintf "%.1f%%" ((float exact / float total) * 100.0)) |> ignore
    summary.AddRow("GTO-Compatible Choices", sprintf "%.1f%%" ((float compatible / float total) * 100.0)) |> ignore
    summary.AddRow("Boundary Hands", sprintf "%d / %d" boundaryHands total) |> ignore
    AnsiConsole.Write(summary)
    AnsiConsole.WriteLine()

    let log =
        Table()
            .Border(TableBorder.Rounded)
            .AddColumn("No")
            .AddColumn("Stack")
            .AddColumn("Pos")
            .AddColumn("Hand")
            .AddColumn("Your")
            .AddColumn("Mix")
            .AddColumn("Result")

    results
    |> List.iter (fun r ->
        let status =
            match r.Grade with
            | BestAction -> "[green]Best[/]"
            | MixedStrategyOkay -> "[yellow]Mixed OK[/]"
            | Incorrect -> "[red]Miss[/]"

        log.AddRow(
            string r.HandNo,
            sprintf "%dbb" r.Entry.Stack,
            r.Entry.Position,
            r.Entry.Hand,
            actionToText r.Chosen,
            formatMix r.Entry,
            status
        ) |> ignore)

    AnsiConsole.MarkupLine("[bold]Hand log[/]")
    AnsiConsole.Write(log)
    AnsiConsole.MarkupLine("\n[grey]Press any key to exit...[/]")
    waitForContinue ()

[<EntryPoint>]
let main args =
    let chart = loadChart ()
    let totalHands = parseHandCount args

    if args |> Array.contains "--audit" then
        runAudit chart
    else
        renderWelcome chart

        let scenarios = List.init totalHands (fun _ -> chooseEntry chart)
        let results =
            scenarios
            |> List.mapi (fun idx entry ->
                let handNo = idx + 1
                renderScenario handNo scenarios.Length entry
                let choice = askPlayerAction ()
                let grade = renderImmediateFeedback entry choice
                { HandNo = handNo
                  Entry = entry
                  Chosen = choice
                  Grade = grade })

        summarizeResults results
    0
