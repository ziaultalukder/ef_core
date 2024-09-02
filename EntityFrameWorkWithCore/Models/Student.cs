﻿namespace EntityFrameWorkWithCore.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }=string.Empty;
        public string Address { get; set; }=string.Empty;
        public int DepartmentId { get; set; }
    }
}
