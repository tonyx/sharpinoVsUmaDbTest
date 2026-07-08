namespace Sharpino.Sample._9

open System
open Sharpino.Commons
open Sharpino.Core
open Sharpino
open FsToolkit.ErrorHandling
open SharpinoVsUma.Definitions
open System.Text.Json

module rec Course =
    let maximumNumberOfTeachers = 3
    type Course =
        {
            Name: string
            Id: Guid
            Students: List<Guid>
            Teachers: List<Guid>
            MaxNumberOfStudents: int
        }
        
        with
            static member MkCourse (name: string, maxNumberOfStudents: int) =
                { Id = Guid.NewGuid(); Name = name; Students = List.empty; Teachers = []; MaxNumberOfStudents = maxNumberOfStudents }
                    
            member this.AddStudent (studentId: Guid) =
                result
                    {
                        do! 
                            (this.Students.Length < this.MaxNumberOfStudents)
                            |> Result.ofBool "course is full"
                        
                        return    
                            {     
                                this
                                    with
                                        Students = this.Students @ [studentId]
                            }
                    }
            member this.AddTeacher (teacherId: Guid) =
                result
                    {
                        do! 
                            (this.Teachers.Length < maximumNumberOfTeachers)
                            |> Result.ofBool "too many teachers"
                        return
                            {
                                this
                                    with
                                        Teachers = this.Teachers @ [teacherId]
                            }
                    } 
                
            member this.RemoveStudent (studentId: Guid) =
                result
                    {
                        do! 
                            this.Students
                            |> List.exists (fun x -> x = studentId)
                            |> Result.ofBool "there is no such student"
                        return
                            {
                                this
                                    with
                                        Students =
                                            this.Students |> List.filter (fun x -> x <> studentId)
                            }
                    }
            
            static member Version = "_01"
            static member StorageName = "_course"
            member this.Serialize = 
                (this, jsonOptions) |> JsonSerializer.Serialize
            static member Deserialize (data: string) =
                try
                    let course = JsonSerializer.Deserialize<Course> (data, jsonOptions)
                    Ok course
                with
                    | ex -> Error ex.Message


   