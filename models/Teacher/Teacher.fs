namespace Sharpino.Sample._9
open System
open Sharpino.Commons
open Sharpino
open Sharpino.Core
open SharpinoVsUma.Definitions
open System.Text.Json

module Teacher =
    let maximumNumberOfCourses = 5
    type Teacher = {
        Id: Guid
        Name: string
        Courses: List<Guid>
    }
    with
        static member MkTeacher (name: string) =
            { Id = Guid.NewGuid(); Name = name; Courses = List.empty }

        member this.AddCourse (courseId: Guid) =
            result
                {
                    do! 
                        this.Courses
                        |> List.length < maximumNumberOfCourses
                        |> Result.ofBool "Maximum number of courses reached"
                    return
                        {
                            this
                                with
                                    Courses = this.Courses @ [courseId]
                        }
                }
            
        member this.RemoveCourse (courseId: Guid) =
            {
                this
                    with
                        Courses = this.Courses |> List.filter (fun x -> x <> courseId)
            }
            |> Ok
            
        static member Version = "_01"
        static member StorageName = "_teacher"
        member this.Serialize = 
            (this, jsonOptions) |> JsonSerializer.Serialize
        static member Deserialize (data: string) =
            try
                let teacher = JsonSerializer.Deserialize<Teacher> (data, jsonOptions)
                Ok teacher
            with
                | ex -> Error ex.Message
        
