module Tests

open UmaDb.Client.ClientBuilder
open UmaDb.Client.Operations
open SharpinoVsUma
open Sharpino.Sample._9.Student
open System.Threading
open System.Threading.Tasks
open UmaDb.Client.ClientBuilder
open UmaDb.Client.Operations
open UmaDb.Client.Event
open UmaDb.Client.Query
open Sharpino.Commons

open Expecto
open SharpinoVsUma.CourseManager
open SharpinoVsUma.Commons
open Sharpino.EventBroker
open Sharpino.CommandHandler
open Sharpino.Sample._9.Course
open Sharpino.Sample._9.CourseEvents
open Sharpino.Sample._9.StudentEvents
open Sharpino.Sample._9.Teacher
open Sharpino.Sample._9.TeacherEvents
open Sharpino.Sample._9.Balance
open Sharpino.Sample._9.BalanceEvents
open Sharpino.Sample._9.StudentCommands
let parallelTasksCount = 10

[<Tests>]
let umaCliTests =
    let courseViewerAsync = fun ct -> getAggregateStorageFreshStateViewerAsync<Course, CourseEvents, string> pgEventStore ct 
    let studentViewerAsync = fun ct -> getAggregateStorageFreshStateViewerAsync<Student, StudentEvents, string> pgEventStore ct 
    let teacherViewerAsync = fun ct -> getAggregateStorageFreshStateViewerAsync<Teacher, TeacherEvents, string> pgEventStore ct 
    let balanceViewerAsync = fun ct -> getAggregateStorageFreshStateViewerAsync<Balance, BalanceEvents, string> pgEventStore ct 
    let mkCourseManagerAsync =
        fun () ->
            CourseManagerAsync(
                pgEventStore,
                MessageSenders.NoSender,
                courseViewerAsync,
                studentViewerAsync,
                teacherViewerAsync,
                balanceViewerAsync,
                Balance.MkBalance 1000.0M
            )

    testList "samples" [
        testCaseTask "write 10000 init events to uma" <| fun _ ->
            task
                {
                    use client = connect "localhost" 50051 |> build

                    let student = Student.MkStudent("student", 2)

                    let students =
                        [ 1.. 10000]
                        |> List.map (fun _ ->  Student.MkStudent("student", 2))

                    let tag = $"order-{student.Id}"

                    let evt =
                        {
                            EventType = "StudentCreated"
                            Tags = None
                            Data = binarySerializer.Serialize<Student> student
                            Id = None
                        }

                    let evts =
                        students
                        |> List.map (fun s ->
                            {
                                EventType = "StudentCreated"
                                Tags = None
                                Data = binarySerializer.Serialize<Student> student
                                Id = None
                            })

                    let sw = System.Diagnostics.Stopwatch.StartNew()
                    let! res = appendOperation  evts|> append client CancellationToken.None
                    sw.Stop()
                    printfn "Uma Append operation took %d ms" sw.ElapsedMilliseconds
                    let query = 
                        [ { Types = ["StudentCreated"]; Tags = [ tag]}]
                    let! events, head = readList client query

                    Expect.isTrue true "true"
                }

        testCaseTask "write 10000 initial elements to Sharpino" <| fun _ ->
            task {
                setUp pgEventStore
                let courseManager = mkCourseManagerAsync ()
                let students =
                    [ 1.. 10000]
                    |> List.map (fun _ ->  Student.MkStudent("student", 2))
                    |> Array.ofList

                let sw = System.Diagnostics.Stopwatch.StartNew()
                let studentsAdded = courseManager.AddStudentsAsync students
                sw.Stop()
                printfn "Add operation took %d ms" sw.ElapsedMilliseconds
                
                Expect.isTrue true "true"
            }

        testCaseTask "create a student and subscribe it to 1000 courses" <| fun _ ->
            task {
                // setUp pgEventStore
                let courseManager = mkCourseManagerAsync ()
                let student = Student.MkStudent("student", 2)
                let studentAdded = courseManager.AddStudentAsync student
                let fakeCourseIds = [ 1.. 10000] |> List.map (fun _ -> System.Guid.NewGuid())
                let sw = System.Diagnostics.Stopwatch.StartNew()
                let! massiveSubscriptions =
                    courseManager.MassiveFakeSubscriptionsAsync (student.Id, fakeCourseIds)
                sw.Stop()
                printfn "Massive Subscription of 10000 courses took %d ms" sw.ElapsedMilliseconds
                
                Expect.isOk massiveSubscriptions "should be ok"
            }

        testCaseTask "write 10000 init events in parallel to UmaDb" <| fun _ ->
            task {
                use client = connect "localhost" 50051 |> build
                let students =
                    [ 1.. 10000]
                    |> List.map (fun _ -> Student.MkStudent("student", 2))
                
                let sw = System.Diagnostics.Stopwatch.StartNew()
                let tasks =
                    students
                    |> List.map (fun student ->
                        task {
                            let evt =
                                {
                                    EventType = "StudentCreated"
                                    Tags = None
                                    Data = binarySerializer.Serialize<Student> student
                                    Id = None
                                }
                            let! res = appendOperation [evt] |> append client CancellationToken.None
                            return res
                        }
                    )
                let! results = Task.WhenAll tasks
                sw.Stop()
                printfn "Parallel Uma Append operation (10000 elements) took %d ms" sw.ElapsedMilliseconds
                Expect.isTrue true "true"
            }

        testCaseTask "write 10000 initial elements in parallel to Sharpino" <| fun _ ->
            task {
                setUp pgEventStore
                let courseManager = mkCourseManagerAsync ()
                let students =
                    [ 1.. 10000]
                    |> List.map (fun _ -> Student.MkStudent("student", 2))
                
                let sw = System.Diagnostics.Stopwatch.StartNew()
                let tasks =
                    students
                    |> List.map (fun student ->
                        courseManager.AddStudentAsync student
                    )
                let! results = Task.WhenAll tasks
                sw.Stop()
                printfn "Parallel Sharpino Add operation (10000 elements) took %d ms" sw.ElapsedMilliseconds
                Expect.isTrue true "true"
            }

        testCaseTask $"write 10000 init events in parallel across {parallelTasksCount} tasks to UmaDb" <| fun _ ->
            task {
                use client = connect "localhost" 50051 |> build
                let sw = System.Diagnostics.Stopwatch.StartNew()
                let tasks =
                    [ 1 .. parallelTasksCount ]
                    |> List.map (fun _ ->
                        task {
                            let students =
                                [ 1.. 10000]
                                |> List.map (fun _ -> Student.MkStudent("student", 2))
                            let evts =
                                students
                                |> List.map (fun student ->
                                    {
                                        EventType = "StudentCreated"
                                        Tags = None
                                        Data = binarySerializer.Serialize<Student> student
                                        Id = None
                                    })
                            let! res = appendOperation evts |> append client CancellationToken.None
                            return res
                        }
                    )
                let! results = Task.WhenAll tasks
                sw.Stop()
                printfn "Parallel tasks (%d tasks of 10000 elements) Uma Append operation took %d ms" parallelTasksCount sw.ElapsedMilliseconds
                Expect.isTrue true "true"
            }

        testCaseTask $"write 10000 initial elements in parallel across {parallelTasksCount} tasks to Sharpino" <| fun _ ->
            task {
                setUp pgEventStore
                let courseManager = mkCourseManagerAsync ()
                let sw = System.Diagnostics.Stopwatch.StartNew()
                let tasks =
                    [ 1 .. parallelTasksCount ]
                    |> List.map (fun _ ->
                        let students =
                            [ 1.. 10000]
                            |> List.map (fun _ -> Student.MkStudent("student", 2))
                            |> Array.ofList
                        courseManager.AddStudentsAsync students
                    )
                let! results = Task.WhenAll tasks
                sw.Stop()
                printfn "Parallel tasks (%d tasks of 10000 elements) Sharpino Add operation took %d ms" parallelTasksCount sw.ElapsedMilliseconds
                Expect.isTrue true "true"
            }
    ]
    |> testSequenced
