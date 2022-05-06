#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#time "on"
#r "nuget: Akka.Remote"

open Akka.FSharp
open System
open System.Text
open Akka
open System.Security.Cryptography

let configuration =
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = 127.0.0.1
                port = 9090
            }
        }"

printfn "Enter leading number of zeroes:"
let inputStr = Console.ReadLine()
let inputZeroes = int inputStr
let UFID = "shubhamsrivastav"
let mutable clientList = []
let actors = 10

let system = System.create "ServerSystem" configuration

let serverActor (mailbox: Actor<_>) =
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string ->
                    if message.Equals("Hello!")
                    then    
                            printfn "Remote Client Up"
                            clientList <- sender :: clientList
                            let extractionDetails = inputStr + "," + UFID
                            sender <! extractionDetails
                    elif message.Equals("Hash Found!")
                    then
                            //printfn "%A" clientList
                            for client in clientList do
                                printfn "Client %A" client
                                client <! "Abort"
                                //(client : Actor<_>) <! "Abort"   
                            Environment.Exit 1
                    else
                        printfn "%s" message
                        // async { sender <! "Terminate" }
                        // |> Async.RunSynchronously
                        Environment.Exit 1
                    return! loop()
                | _ ->  failwith "Unknown message"
            }
        loop()

let serverRef = spawn system "serverActor" serverActor

let replaceDashes (hash : string) = 
    hash.Replace("-","")

let CheckLeadingZeroes (hashed_string: string) = 
    hashed_string.[..(inputZeroes-1)]
    |> Seq.forall (fun c -> c = '0')

let generateRandomStrings (sizeOfRandomString : int) = 
    let r = Random()
    let charArray = (Array.concat([[|'a' .. 'z'|];[|'A' .. 'Z'|];[|'0' .. '9'|]]))
    let sz = Array.length charArray in
    let randomString = String(Array.init sizeOfRandomString (fun _ -> charArray.[r.Next sz]))
    let finalString = UFID + ";" + randomString
    //let hashedValue = finalString |> Encoding.ASCII.GetBytes |>(new SHA256CryptoServiceProvider()).ComputeHash |> System.BitConverter.ToString |> replaceDashes
    finalString

let generateHashForRandomStrings (finalString : string) = 
    let hashedValue = finalString |> Encoding.ASCII.GetBytes |>(new SHA256CryptoServiceProvider()).ComputeHash |> System.BitConverter.ToString |> replaceDashes
    hashedValue

let genRandomNumbers count = 
    let rnd = System.Random()
    let initial = List.init count (fun _ -> rnd.Next(100,500))
    initial
    |> Seq.distinct
    |> Seq.take(count)
  //  |> Seq.ofList

let workerActor (mailbox: Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        printfn "Processing.."
        let sizeOfRandomString = message
        let mutable looping = true 
        // Handle message here
        while looping do
            let randomString = generateRandomStrings sizeOfRandomString
            let hashString = generateHashForRandomStrings randomString
            if CheckLeadingZeroes(hashString) then
                looping <- false
                let bitCoin = randomString + "\t" + hashString
                printfn "%s" bitCoin
                serverRef <! "Hash Found!"
                //sender <! "Hash Found!"      
            else
                looping <- true
        return! loop ()
    }
    loop ()

// Agent 1 for processing the hash
let parentActor actors (mailbox: Actor<_>) = 
    actor {
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        // Handle message here
     //   if(message.Equals("Hash Found!")) then Environment.Exit 1
        let randomNumberList = genRandomNumbers actors
        randomNumberList  |> Seq.iter (fun x ->
            let head = x
            let workerName = "worker:" + string head
            let workerRef = spawn system workerName workerActor
            async { workerRef <! head }
            |> Async.RunSynchronously)
    }

let parentRef = spawn system "parentActor" (parentActor actors)

if inputZeroes > 0
then parentRef <! "Execute Parent Actor"

Console.ReadLine() |> ignore