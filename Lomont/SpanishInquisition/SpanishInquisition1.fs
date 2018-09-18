open System
open System.IO
open System.Diagnostics

// Chris Lomont Rock-Paper-Scissors bot

//********************* Utility *************************************

let moveStrings = [|"ROCK";"PAPER";"SCISSORS"|]
let beats = [|1;2;0|] // index i is who beats move i

// score.[i*3+j] is +1 if i beats j, -1 if j beats i, else 0
let score = 
    [|
    +0; -1;  1;  // rock     : ...
    +1;  0; -1;  // paper    : ...
    -1;  1;  0   // scissors : ...
    |]


// compute score over all history
let computeScore selfHistory otherHistory = 
    List.fold (fun acc (s,o) -> acc+(score.[s*3+o])) 0 (List.zip selfHistory otherHistory)

//********************* Log *****************************************
let logFile = new StreamWriter("SpanishLog.txt")
let dumpLog (msg:string) (items:list<int>) = 
    logFile.Write(msg)
    (items |> List.map (fun n -> logFile.Write("{0}, ",n))) |> ignore
    logFile.WriteLine();

//********************* Predictors **********************************
// predictors take played and opponent history and return a predicted move
// some take parameters, some take nothing

// return random move 0,1,2 using local state
let randPredict = 
    let rand = Random()
    fun _ -> rand.Next(3)

let frequencyPredict length playedHistory opponentHistory  = 
    // todo - up to given move length
    let count0 = Seq.length (Seq.filter(fun m -> m=0) opponentHistory)
    let count1 = Seq.length (Seq.filter(fun m -> m=1) opponentHistory)
    let count2 = Seq.length (Seq.filter(fun m -> m=2) opponentHistory)
    let counts = [count0;count1;count2]
    // return index of max value
    fst (counts
        |> Seq.mapi(fun i v -> i,v) // index and value
        |> Seq.maxBy snd
        )

let matchLength (items:list<int>) start1 start2 maxLength = 
    let len = List.length items
    let rec counter curLen = 
        let i1,i2 = start1+curLen,start2+curLen
        if curLen = maxLength || i1 >= len || i2 >= len || items.[i1] <> items.[i2] then
            curLen
        else
            counter (curLen+1)
    counter 0

let historyPredict paired length playedHistory opponentHistory  = 
    let step,history = match paired with 
       | false -> 1 , opponentHistory
       | true  -> 2 ,(List.zip opponentHistory playedHistory)|> List.collect (fun (a,b)-> [a;b])
    let len = List.length history// todo - match also on played history??
    if len < step*2 then
        randPredict()
    else
        // get length at each start position step by step matching to start
        let runs = Seq.mapi(fun i s -> i, matchLength history 0 s length) (seq{step..length})

        // return index of max value
        let maxIndex = step + (fst (runs |> Seq.maxBy snd))
        let pred = 
            match maxIndex with
            | 0 -> randPredict()
            | _ -> history.[maxIndex-step]
        
        //logFile.WriteLine("Max index {0} pred {1} opp last {2}", maxIndex, moveStrings.[pred],moveStrings.[opponentHistory.Head])
        pred


//********************* Meta strategy *******************************
// convert a predictor into 6 varying strategies covering all cases
// of second guessing, and of using similar predictors against this program
let makeStrategies predictor = 
    seq {
    // direct, double, triple guessing
    yield fun hist1 hist2 -> beats.[predictor hist1 hist2]
    yield fun hist1 hist2 -> beats.[beats.[predictor hist1 hist2]]
    yield fun hist1 hist2 -> beats.[beats.[beats.[predictor hist1 hist2]]]
    // reversed
    yield fun hist2 hist1 -> beats.[predictor hist1 hist2]
    yield fun hist2 hist1 -> beats.[beats.[predictor hist1 hist2]]
    yield fun hist2 hist1 -> beats.[beats.[beats.[predictor hist1 hist2]]]
    }

//********************* Move generation *****************************

// return updated strategies, histories, and a best move
let generateMove (strategies,playedHistory:list<int>,opponentHistory) = 
    // todo - do scores over 10 20 50 length, pick then, helps change tactics faster
    let scores        = List.map(fun s-> computeScore (snd s) opponentHistory) strategies
    let moves         = List.map(fun s-> (fst s) playedHistory opponentHistory) strategies

    //if playedHistory.Length > 0 then
    //    logFile.WriteLine("Last pair {0} {1}",moveStrings.[playedHistory.Head], moveStrings.[opponentHistory.Head])

    //dumpLog "Scores: " scores
    //dumpLog "Moves: "  moves
    let newStrategies = List.map(fun (s,m) -> ((fst s),m::snd s)) (List.zip strategies moves)
    let bestMove = snd ( (Seq.zip scores moves) |> Seq.maxBy fst )

    //logFile.WriteLine("Best move {0}",moveStrings.[bestMove])


    ((newStrategies, bestMove::playedHistory, opponentHistory),bestMove)


//********************* console interface ***************************

let stopwatch = new Stopwatch()

let rec gameRunner stateOption = 
    let ticks = (stopwatch.ElapsedTicks)&&&1L
    if ticks = 0L then
        randPredict() |> ignore // scramble a bit
    match stateOption with 
    | None        -> ()
    | Some(state) ->
        let strategies, playedHistory, opponentHistory = state
        let words = Console.ReadLine().ToLower().Split() |>Array.toList
        //logFile.WriteLine("Got line {0}",words.[0])
        let next = 
            match words with 
            | "reset"    :: name -> 
                //logFile.WriteLine("Player {0}",name)
                Some(generateMove (List.map(fun s -> (fst s,[])) strategies, [], []))                
            | "rock"     :: _ -> Some(generateMove (strategies, playedHistory,  0::opponentHistory))
            | "paper"    :: _ -> Some(generateMove (strategies, playedHistory,  1::opponentHistory))
            | "scissors" :: _ -> Some(generateMove (strategies, playedHistory,  2::opponentHistory))
            | "quit"     :: _ -> None // Console.WriteLine "No one expects the Spanish Inquisition!"; None
            | _               -> None 
        match next with 
        | None    -> ()
        | Some(newState,move) -> 
            //logFile.WriteLine()
            Console.WriteLine(moveStrings.[move])
            gameRunner (Some newState)


[<EntryPoint>]
let main argv = 
    let freqLengths = [1;10;50;250] // lengths to analyze
    let histLengths = [25]         // lengths to analyze
    // each strategy is a predictor and history
    let strategies = 
        seq {
        yield! Seq.concat (Seq.map(fun len -> (makeStrategies (frequencyPredict len))) freqLengths)
        yield! Seq.concat (Seq.map(fun len -> (makeStrategies (historyPredict true  len))) histLengths)
        yield! Seq.concat (Seq.map(fun len -> (makeStrategies (historyPredict false len))) histLengths)
        yield   (fun _ _ -> randPredict())
//        yield (fun h1 h2 -> beats.[(frequencyPredict 20) h1 h2])
//        yield (fun h1 h2 -> beats.[(historyPredict false 20) h1 h2])
        // yield! Seq.concat (Seq.map(fun len -> (makeStrategies (historyPredict true  len))) histLengths)
        //yield! Seq.concat (Seq.map(fun len -> (makeStrategies (historyPredict false len))) histLengths)
        } |>Seq.map(fun f -> (f,[])) |>Seq.toList

    stopwatch.Start()
    gameRunner (Some (strategies,[],[])) |> ignore
    logFile.Close();

    1 // return value


