using SkPlayground.Models;

namespace SkPlayground.Util.Helpers;

public static class FactHelper
{
  public static IEnumerable<Fact> GetFacts()
  {
    var facts = new Fact[]
    {
        new("I was born in Berlin.", "Place of birth", "City: Berlin"),
        new("I am 25 years old.", "Age", "Years: 25"),
        new("My favorite sports team is Lakers.", "Favorite sports team", "Team: Lakers"),
        new("I have 2 siblings.", "Siblings", "Number: 2"),
        new("I work as a developer.", "Occupation", "Job Title: Developer"),
        new("I enjoy hiking.", "Hobbies", "Activity: Hiking"),
        new("I have a pet dog.", "Pets", "Type: Dog"),
        new("My favorite cuisine is Italian.", "Favorite Cuisine", "Cuisine: Italian"),
        new("I have visited 5 countries.", "Travel", "Countries: 5"),
        new("I graduated from the University of London.", "Education", "University: London"),
        new("I speak 3 languages.", "Languages Spoken", "Number: 3"),
        new("I am allergic to peanuts.", "Allergies", "Allergen: Peanuts"),
        new("I have run a marathon.", "Athletic Achievements", "Event: Marathon"),
        new("I have a collection of vintage stamps.", "Collections", "Item: Stamps"),
        new("I prefer autumn over other seasons.", "Seasonal Preferences", "Season: Autumn"),
        new("My favorite book is 'To Kill a Mockingbird'.", "Favorite Book", "Book: To Kill a Mockingbird"),
        new("I am a vegetarian.", "Diet", "Diet: Vegetarian"),
        new("I have volunteered at a local shelter.", "Volunteering", "Place: Local Shelter"),
        new("I have a goal to visit every continent.", "Life Goals", "Goal: Visit Every Continent"),
        new("I play the guitar.", "Musical Instruments", "Instrument: Guitar"),
        new("I have a master's degree in Computer Science.", "Advanced Education", "Degree: Master's in Computer Science")
    };
    return facts;
  }
}