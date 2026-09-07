using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

//Simple Business object.

public class Person
{
    public Person(string fName, string lName)
    {
        this.firstName = fName;
        this.lastName = lName;
    }
    public string firstName;
    public string lastName;
}
// Collection of Person Objects.
// This class impelemnets IEnumerable so that it can be used with ForEach syntax.

public class People : IEnumerable
{
    private Person[] _people;
    public People(Person[] pArray)
    {
        _people = new Person[pArray.Length];
        for (int i = 0; i < pArray.Length; ++i)
        {
            _people[i] = pArray[i];
        }

    }
    //Implementation of the GetEnumerator method
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }
    public PeopleEnum GetEnumerator()
    {
        return new PeopleEnum(_people);
    }
}
//when you implement IEnumerable, you must all implement IEnumerator
public class PeopleEnum : IEnumerator
{
    public Person[] _people;
    //Enumerators are positioned BEFORE the first element, untilt the FIRST MoveNext() call.
    int position = -1;
    public PeopleEnum(Person[] list)
    {
        _people = list;
    }
    public bool MoveNext()
    {
        position++;
        return (position < _people.Length);
    }
    public void Reset()
    {
        position = -1;
    }
    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }
    public Person Current
    {
        get
        {
            try
            {

                return _people[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }

        }

    }
};

class App
{
    static void Main()
    {
        Person[] peopleArray = new Person[3]{
            new Person("John", "Smith"),
            new Person("Matthew","Yu"),
            new Person("Michelle", "Yu"),
        };
        People peopleList = new People(peopleArray);
        foreach (Person p in peopleList)
        {
            Console.WriteLine(p.firstName + " " + p.lastName);
        }
    }



}