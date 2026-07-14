// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hi there! Welcome to DevConnect API!");
using System;

public class Vehicle{
   public string Name;
   public string Color;
   public string Model;
//    public Vehicle(string name, string color, string model){
//       Console.WriteLine("Vehicle is created");
//       this.Name = name;
//       this.Color = color;
//       this.Model = model;
//    }

public void updateVehicle(string name, string color, string model){
    this.Name = name;
    this.Color = color;
    this.Model = model;
}

}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hi there! Welcome to DevConnect API!");
    //    Vehicle car = new Vehicle("Toyota", "Red", "2024");
     Vehicle car = new Vehicle();
     car.updateVehicle("Toyota", "Red", "2024");

       Console.WriteLine("Vehicle Name: " + car.Name);
       Console.WriteLine("Vehicle Color: " + car.Color);
       Console.WriteLine("Vehicle Model: " + car.Model);

        /*
        What is LINQ?
        Language Integrated Query (LINQ) is a feature in C# that allows developers to write queries directly in the C# language to retrieve and manipulate data from various data sources, such as collections, databases, XML, and more.
        It provides a consistent and readable syntax for querying data, making it easier to work with different types of data sources.
        */

        List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        //Without LINQ
        List<int> evenNumbersWithoutLinq = new List<int>();
        foreach (int number in numbers)
        {
            if (number % 2 == 0)
            {
                evenNumbersWithoutLinq.Add(number);
            }
        }
        Console.WriteLine("Even Numbers without LINQ: " + );
    }
}


