# Project 1
## COP5615 Distributed Operating Systems Principles

#### Group Members
* Akshay Sharma (UFID - 56006840)
* Shubham Srivastava (UFID - 27599322)

### Description

The project is inspired by the bitcoin mining process and we will try to achieve similar computational work using distirbuted operating systems.
This project will implement ACTOR model in F# language using AKKA.net API. The ACTOR model will work as distributed system to find a hashed value (basically a string) that will have a smaller or equal value to the target hash and will contain some leading zeroes that will be given by the user. The target The hash algorithm we are using is SHA-256. We will also implement a basic client-server model to distribute work to multiple machines. 

### Work Distribution


The project contains two files namely:

* **Program.fsx**

This file will work as a server for our project and server will distribute work to the client. The server as well will work on finding the hash value. We will take a random alpha numeric string that will be concatenated with the gatorlink id along with a ";" in between. For generating a random string, different actors will recieve a random unique number which will be used as a fix size for the random string for that particular actor. Similarly different actors will generate hash values with random strings of a distinct length. Each server will listen for the client request on a fixed ip address and port (127.0.0.1, 9091). 

The server will stop the process of itself as well as all the clients if it will find a hash value.

* **LocalActor.fsx**

This file will work as a client for our project. The client will send a connection request to the server on a fixed local host with ip address (127.0.0.1) and port (9091). Once the connection is established the server will distribute the work to it.

If a client will find a hash value then it will signal the server the same which will stop all the processes.


### Running the project

As we have two files for the project, we can run the files on a singles machine using two separate terminals or we can run the project on two separate machines running each file at a time.

***libraries required***

Install the following :
```
	F# (v5.0 or greater)
	Nuget
	Akka.net API
```

Following commands should be added to both the files otherwise there will be an error:

```FSharp
	#r "nuget: Akka.FSharp" 
	#r "nuget: Akka.TestKit"
	#r "nuget: Akka.Remote"
```

Command to execute the program, run the following command into the terminal

` dotnet fsi Program.fsx` -> for server file
` dotnet fsi LocalActor.fsx` -> for client file

**program may take large amount of time to execute but this can vary depending on the architecture or also the user given input**


**Input**

The user will be required to input a number that will be equal to the leading numbers of zeroes that should be in the final hashed value.

Sample Input

```
5
```

**Output**

The output will be the hash value with given leading number of zeroes.

### Results

The result of running the program for leading zeroes = 4. 

![Alt text](./input4.png?raw=true "Input 8")

Real time and cpu time analysis with input = 4

```
Real time = 11.54s
CPU time = 12.65s
Real time / CPU time = 1.09
```

**The coin with the most 0s**

Input = 7

![Alt text](./input7.png?raw=true "Input 8")

```
Real time = 148.57s
CPU time = 464.26s
Real time / CPU time = 3.12485697
```
