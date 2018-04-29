﻿/// This module contains functions which allow to report unit test results to build servers.
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
module Fake.UnitTestHelper

open System
open Fake

/// Basic data type to represent test status
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
type TestStatus = 
    | Ok
    | Ignored of string * string
    | Failure of string * string

/// Basic data type to represent tests
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
type Test = 
    { Name : string
      RunTime : TimeSpan
      Status : TestStatus }

/// Basic data type to represent test results
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
type TestResults = 
    { SuiteName : string
      Tests : Test list }
    [<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]    
    member this.GetFailed() = 
        this.Tests |> List.filter (fun t -> 
                          match t.Status with
                          | TestStatus.Failure _ -> true
                          | _ -> false)
    
    [<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
    member this.GetTestCount() = List.length this.Tests

/// Reports the given test results to [TeamCity](http://www.jetbrains.com/teamcity/).
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
let reportToTeamCity testResults = 
    StartTestSuite testResults.SuiteName
    for test in testResults.Tests do
        let runtime = System.TimeSpan.FromSeconds 2.
        StartTestCase test.Name
        match test.Status with
        | Ok -> ()
        | Failure(msg, details) -> TestFailed test.Name msg details
        | Ignored(msg, details) -> 
            tracef "ignored with %s %s %s" test.Name msg details
            IgnoreTestCaseWithDetails test.Name msg details
        FinishTestCase test.Name test.RunTime
    FinishTestSuite testResults.SuiteName


/// Reports the given test results to [AppVeyor](http://www.appveyor.com/).
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
let reportToAppVeyor testResults =     
    AppVeyor.StartTestSuite testResults.SuiteName
    for test in testResults.Tests do
        let runtime = System.TimeSpan.FromSeconds 2.
        AppVeyor.StartTestCase testResults.SuiteName test.Name
        match test.Status with
        | Ok -> AppVeyor.TestSucceeded testResults.SuiteName test.Name
        | Failure(msg, details) -> AppVeyor.TestFailed testResults.SuiteName test.Name msg details
        | Ignored(msg, details) -> AppVeyor.IgnoreTestCase testResults.SuiteName test.Name msg
        AppVeyor.FinishTestCase testResults.SuiteName test.Name test.RunTime
    AppVeyor.FinishTestSuite testResults.SuiteName

/// Reports the given test results to the detected build server
[<System.Obsolete("This function, type or module is obsolete. There is no alternative in FAKE 5 yet. If you need this functionality consider porting the module (https://fake.build/contributing.html#Porting-a-module-to-FAKE-5).")>]
let reportTestResults testResults = 
    match buildServer with
    | TeamCity -> reportToTeamCity testResults
    | AppVeyor -> reportToAppVeyor testResults
    | _ -> 
        tracefn "TestSuite: %s" testResults.SuiteName
        for test in testResults.Tests do
            let runtime = System.TimeSpan.FromSeconds 2.
            tracef "Test: %s ==> " test.Name
            match test.Status with
            | Ok -> tracefn "OK"
            | Failure(msg, details) -> tracef "failed with %s %s" msg details
            | Ignored(msg, details) -> tracef "ignored with %s %s" test.Name msg
            FinishTestCase test.Name test.RunTime
