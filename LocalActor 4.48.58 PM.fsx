#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#time "on"
#r "nuget: Akka.Remote"

open Akka.FSharp
open System
open System.Text
open Akka.Actor
open System.Security.Cryptography
open Akka.Configuration

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                deployment {
                    /remoteecho {
                        remote = ""akka.tcp://ServerSystem@10.20.124.9:9090""
                    }
                }
            }
            remote {
                helios.tcp {
                    port = 9091
                    hostname = 127.0.0.1
                }
            }
        }")

let system = ActorSystem.Create("ClientSystem", configuration)
let echoClient = system.ActorSelection("akka.tcp://ServerSystem@10.20.124.9:9090/user/serverActor")
//let echoClient = select ("akka.tcp://ServerSystem@127.0.0.1:9090/user/serverActor") system



//let mutable task : Async<obj>= echoClient <? "Hello!"
//let response = Async.RunSynchronously (task, -1)
//let responseArray = string(response).Split ','
//printfn "Reply from remote %s" (string(response))

// Code to mine Bitcoins
let mutable inputZeroes = 0
let mutable UFID = ""
//let inputZeroes = int inputStr
let actors = 10
let mutable workerNameList = []

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
    let initial = Seq.initInfinite(fun _ -> rnd.Next(100,500))
    initial
    |> Seq.distinct
    |> Seq.take(count)

let workerActor (mailbox: Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
      //  printfn "Processing.."
        let sizeOfRandomString = message
        let mutable looping = true 
        // Handle message here
        while looping do
            let randomString = generateRandomStrings sizeOfRandomString
            let hashString = generateHashForRandomStrings randomString
            if CheckLeadingZeroes(hashString) then
                looping <- false
                //printfn "%s" hashString
                //printfn "Hash Found! by %i" message
                let bitCoin = randomString + "\t" + hashString
                echoClient <! bitCoin
                for workerName in workerNameList do
                    system.Stop(workerName)
                Environment.Exit 1
            else
                looping <- true
        return! loop ()
    }
    loop ()


// Agent 1 for processing the hash
let clientActor actors (mailbox: Actor<_>) = 
    let rec loop () = actor {
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        let responseArray = string(message).Split ','
        inputZeroes <- int responseArray.[0]
        UFID <- responseArray.[1]
        let randomNumberList = genRandomNumbers actors
        //|> Seq.map (fun x -> (Seq.ofList x))
        randomNumberList  |> Seq.iter (fun x ->
            let head = x
            let workerName = "worker:" + string head
           // workerNameList <- workerName :: workerNameList
            let workerRef = spawn system workerName workerActor
        
        //    workerNameList <- workerRef :: workerNameList
            workerNameList <- workerRef :: workerNameList
            async { workerRef <! head }
                    
            |> Async.RunSynchronously)
        return! loop ()
    }
    loop ()

let clientRef = spawn system "clientActor" (clientActor actors)



let simpleConnectionActor (mailbox: Actor<_>) =  
        let rec loop () = actor {
            let! message = mailbox.Receive ()
            let sender = mailbox.Sender()
           // printfn "I am inside loop"
            match box message with
                    | :? string ->
                        if message.Equals("Hello!")
                        then    
                                echoClient <! "Hello!"
                        elif message.Equals("Abort")
                        then
                                Environment.Exit 1 
                        else
                            clientRef <! message
                        return! loop()
                    | _ ->  failwith "Unknown message"
            }
        loop()

let simpleConnectionActorRef = spawn system "simpleConnectionActor" simpleConnectionActor 
simpleConnectionActorRef <! "Hello!"

Console.ReadLine() |> ignore