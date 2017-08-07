[![Build Status](https://travis-ci.org/unosquare/swan.svg?branch=master)](https://travis-ci.org/unosquare/swan)
[![Build status](https://ci.appveyor.com/api/projects/status/063tybvog8mb1sic/branch/master?svg=true)](https://ci.appveyor.com/project/geoperez/swan/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/unosquare/swan/badge.svg?branch=master)](https://coveralls.io/github/unosquare/swan?branch=master)
# <img src="https://github.com/unosquare/swan/raw/master/swan-logo-32.png"></img> SWAN: Stuff We All Need

*:star: Please star this project if you find it useful!*

SWAN stands for Stuff We All Need

Repeating code and reinventing the wheel is generally considered bad practice. At Unosquare we are committed to beautiful code and great software. 
Swan is a collection of classes and extension methods that we and other good developers have developed and evolved over the years. We found ourselves copying and pasting 
the same code for every project every time we started it. We decide to kill that cycle once and for all. This is the result of that idea.
Our philosophy is that SWAN should have no external dependencies, it should be cross-platform, and it should be useful.

NuGet Installation:
-------------------

[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.svg)](https://badge.fury.io/nu/Unosquare.Swan)

```
PM> Install-Package Unosquare.Swan
```

We have a nuget including helpful providers for `AspNetCore` at:

[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.AspNetCore.svg)](https://badge.fury.io/nu/Unosquare.AspNetCore.Swan)

```
PM> Install-Package Unosquare.Swan.AspNetCore
```

## What's in the library

In this section we present the different components that are available in the SWAN library. Please keep in mind that everything in the library is opt-in.
SWAN won't force you to use any of its components, classes or methods.

### The `Runtime`

`Runtime` provides properties and methods that provide information about the application environment (including Assemblies and OS) and access to singleton instance of other components inside Swan as `ObjectMapper`.

[Runtime Documentation](https://unosquare.github.io/swan/api/Unosquare.Swan.Runtime.html)

### The `Terminal`

Many times we find ourselves implementing `Console` output code as some NLog or Log4Net logger or adapter, especially 
when writing console applications, daemons and windows services. We also tend to write `Console` code for reading user 
input because it can't be some logger or adapter. And then you have the `System.Diagnostics.Debug` class to write 
to the debugger output. And finally, all your `Console` user interaction looks primitive and unprofessional. In other 
words, you end up with 3 things you are unsure of how to put together in the different configurations and runtime environments:
`Console`, `Debug` and some logging mechanism. In return you have placed unintended logging code, `Console` code, and `Debug` 
code everywhere in your application and it makes it look silly, bloated and written by an amateur.  

The Swan `Terminal` is __all__ of the following:
- Console Standard Output Writer
- Console Standard Error Writer 
- Debug Writer
- Console Standard Input Reader
- Log message forwarder

It is also very easy to use, it's thread-safe, and it does not rquire you to learn anything new. In fact it simplifies logging
messages and diplaying `Console` messages by providing `string` extension methods.

[Terminal Documentation](https://unosquare.github.io/swan/api/Unosquare.Swan.Terminal.html)

#### Example 1: Writing to the Terminal

This only writes messages out to the `TerminalWriters` if they are avialble. In practice, we typically **DO NOT** use
the `Write` and `WriteLine` methods but they are provided for convenience, extensibility and customization. Please note
that these methods do not forward messages as logging events and therefore whatever is written via these methods
will not show up in you logging subsystem.

```csharp
// The simplest way of writing a line of text -- equivalent to `Console.WriteLine`:
Terminal.WriteLine($"Hello, today is {DateTime.Today}");

// A slightly better way using extension methods:
$"Hello, today is {DateTime.Today}".WriteLine();

// Now, let's add some color:
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green);

// Write it out to the debugger as well!
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);

// You could have also set the color argument to null and just use the configured default
$"Hello, today is {DateTime.Today}".WriteLine(null, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);
```

#### Example 2: Basic Logging

This is where `Terminal` really shines. Instead of using the `Write` and `WriteLine` methods, you can use the 
methods that are intended for logging. These methods have different purposes and distinct functionality. Please
refer to the example below and its comments.

```csharp
$"Hello, today is {DateTime.Today}".Info();
```

#### Example 3: Forwarding Logging Messages

Suppose you have various calls to `Terminal`'s logging methods such as `Info()`, `Warn()`, `Error()`, `Trace()`
and `Debug()`. You wish to forward those messages to a logging subsystem in addition to using the `Console`'s
standard output and standard error, and the built-in diagnostics output. All you have to do is subscribe to the
Terminal's `OnLogMessageReceived` event. The event arguments of this event provide useful properties that you
can piece together to send your logging messages directly to the Logging subsystem in your application.

#### Example 4: Configuring Output

Swan's `Terminal` provides both, flexibility and consistency for all of its output. While it will pick the most
common defaults for a given build or runtime scenario, you are able to modify such defaults and adjust them to your
liking. You can change the output colors,  

#### Example 5: User Interaction

The Swan `Terminal` would not be complete without a way to read user input. The good news is
that `Terminal` can create decent-looking user prompts if a very convenient way.

```csharp
// TODO: These examples need work.

var lineResult = Terminal.ReadLine();

var numberResult = Terminal.ReadNumber();

var promptResult = Terminal.ReadPrompt();

var keyResult = Terminal.ReadKey();
``` 

#### Example 6: Other Useful Functions

Swan's `Terminal` also provides additional methods to accomplish very specific tasks. Given the fact that `Terminal`
is an asynchronous, thread-safe output queue, we might under certain situations require all of the output queue to be written
out to the `Console` before the program exits. For example, when we write a console application that requires its usage
to be fully printed out before the process is terminated. In these scenarios we use `Terminal.Flush` which blocks
the current thread until the entire output queue becomes empty.

### The `Json`

You can serialize and deserialize strings and objects using Swan's `Json` Formatter. It's a great way to transform objects to JSON format and vice versa. For example, you need to send information as JSON format to other point of your application and also when arrives it's necessary to get back to the object that is going to be used, and thanks to JSON format the data can interchange in a lightweight way.

[Json Documentation](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.Json.html)

#### Example 1: Serialize

Serializes the specified object into a JSON `string`.

```csharp
// The object to be serialize
var basicObject = new { One = "One", Two = "Two", Three = "Three" };
// Serializes the specified object into a JSON string.
var data = Json.Serialize(basicObject);
```

#### Example 2: Serialize included properties

Serializes the specified object only including the specified property names.

```csharp
// The object to be serialize
var basicObject = new { One = "One", Two = "Two", Three = "Three" };
// The included names
var includedNames  = new[] { "Two", "Three" };
// Serialization Only.
var data = Json.SerializeOnly(basicObject, true, includedNames);
```

#### Example 3: Serialize excluding properties

Serializes the specified object excluding the specified property names.

```csharp         
// The object to be serialize
var basicObject = new { One = "One", Two = "Two", Three = "Three" };
// The excluded names
var excludeNames  = new[] { "Two", "Three" };
// Serialization Excluding
var data = Json.SerializeExcluding(basicObject, true, excludeNames);
``` 
#### Example 4: Deserialize

Deserializes the specified JSON `string` as either a `Dictionary<string, object>` or as a `List<object>` depending on the syntax of the JSON `string`.

```csharp 
// The json to be deserialize
var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
// Deserializes the specified json into Dictionary<string, object>.
var data = Json.Deserialize(basicJson);
``` 

#### Example 5: Deserialize a generic type `<T>`

Deserializes the specified json `string` and converts it to the specified object type. Non-public constructors and property setters are ignored.

```csharp 
// The json Type BasicJson to be deserialize
var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
// Deserializes the specified string in a new instance of the type BasicJson.
var data = Json.Deserialize<BasicJson>(basicJson);
``` 

### The `CsvWriter`

Many projects require the use o CSV files to export the information, with `CsvWriter` you can easily write objects and data to CSV format, also gives a useful way to save the information into a file.

[CsvWriter Documentation](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvWriter.html)

#### Example 1: Writing a List of objects

This is the way to write a list of objects into a CSV format.

```csharp
 // The list of objects to be written as CSV
var basicObj = new List<BasicJson>();

using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(basicObj.ToString())))
{               
    // The writer of the CSV
    var reader = new CsvWriter(stream);
};
```

#### Example 2: Writing a List of objects into a file

You also can write the object into a file or a temporal file.

```csharp
// The list of objects to be written as CSV
var basicObj = new List<BasicJson>();
// This is where the object is save into a file
CsvWriter.SaveRecords<BasicJson>(basicObj, "C:/Users/user/Documents/CsvFile");
```

### The `CsvReader`

When you use and manage the information through CSV files you need to have an easy way to read and load the data into lists and information usable by the application. Swan makes use of `CsvReader` to read and load CSV files.

[CsvReader Documentation](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvReader.html)

#### Example 1: Reading a CSV data format

This is a way to read CSV format data.

```csharp
 // The data to be readed
var data = @"Company,OpenPositions,MainTechnology,Revenue
    Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
    Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
{               
    // The reader of the CSV
    var reader = new CsvReader(stream, true, Encoding.ASCII);
};
```

#### Example 2: Reading a CSV file

From a CSV file you can read and load the information to a generic list.

```csharp
// The list of object to be written as CSV
var basicObj = new List<BasicJson>();
// This is where the object is save into a file
CsvWriter.SaveRecords<BasicJson>(basicObj, "C:/Users/user/Documents/CsvFile");
// This is how you can load the records of the CSV file
var loadedRecords = CsvReader.LoadRecords<BasicJson>("C:/Users/user/Documents/CsvFile");
``` 

### The `JsonClient`

Represents a wrapper `HttpClient` with extended methods to use with JSON payloads and bearer tokens authentication.

[JsonClient](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.JsonClient.html)

#### Example 1: Authentication

You can Authentication into your application.

```csharp
// The Authenticate
var data = JsonClient.Authenticate("https://mywebsite.com/api/token", "admin", "password");
```

#### Example 2: Making a GET

Easy way to make a HTTP GET using `JsonClient`.

```csharp
// The GET
var data = JsonClient.Get<BasicJson>("https://mywebsite.com/api/data");
```

#### Example 3: Making a POST 

Easy way to make a POST using `JsonClient`.

```csharp
// The POST
var data = JsonClient.Post<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

#### Example 4: Making a PUT

Easy way to make a PUT using `JsonClient`.

```csharp
// The PUT
var data = JsonClient.Put<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

### The `SmtpClient`

It's a Swan's basic SMTP client that is capable of submitting messages to an SMTP server. It's very easy to manage an provide a very handly way to make use of SMTP in your application to send mails to any registered user.

[SmtpClient](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.SmtpClient.html)

#### Example 1: Sending mails

The mails are sent asynchronously.

```csharp
// Sending mails async
await client.SendMailAsync(new MailMessage());

// Or sent the mail based on the Smtp session state
await client.SendMailAsync(new SmtpSessionState());
```

### The `ObjectMapper`

It's a very handly component of Swan that maps objects. You can access a default instance of `ObjectMapper` by `Runtime` class.

[ObjectMapper](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectMapper.html)

#### Example 1: Mapping with default map

The conversion generates a map automatically between the properties in base of the properties names.

```csharp
// Here is mapping the specific user to a destination
var destination = Runtime.ObjectMapper.Map<UserDto>(user);
```

#### Example 2: Mapping with a custom map

With `CreateMap` you generates a new map and you can map one custom property with `MapProperty`.

```csharp
// Creating an Object Mapper
var mapper = new ObjectMapper();
// Creating the map and mapping the property
mapper.CreateMap<User, UserDto>().MapProperty(d => d.Role, s => s.Role.Name);
// Then you map the custom map to a destination
var destination = mapper.Map<UserDto>(user);            
```

#### Example 3: Removing a property from the map

To remove a custom property you also use `CreateMap` and then remove the costum property of the mapping.

```csharp
// Create an Object Mapper
var mapper = new ObjectMapper();
// Creating a map and removing a property
mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t.Name);
// Then you map the custom map to a destination
var destination = mapper.Map<UserDto>(user);
```
