[![Build Status](https://travis-ci.org/unosquare/swan.svg?branch=master)](https://travis-ci.org/unosquare/swan)
[![Build status](https://ci.appveyor.com/api/projects/status/063tybvog8mb1sic/branch/master?svg=true)](https://ci.appveyor.com/project/geoperez/swan/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/unosquare/swan/badge.svg?branch=master)](https://coveralls.io/github/unosquare/swan?branch=master)
# <img src="https://github.com/unosquare/swan/raw/master/swan-logo-32.png"></img> SWAN: Stuff We All Need

*:star: Please star this project if you find it useful!*

SWAN stands for Stuff We All Need

Repeating code and reinventing the wheel is generally considered bad practice. At Unosquare we are committed to beautiful code and great software. Swan is a collection of classes and extension methods that we (and other good developers) have written and evolved over the years. We found ourselves copying and pasting the same code for every project every time we started them. We decided to kill that cycle once and for all. This is the result of that idea. Our philosophy is that Swan should have no external dependencies, it should be cross-platform, and it should be useful.

 Table of contents
=================
  * [Libraries](#libraries)
  * [Installation](#installation)
  * [Whats in the library](#whats-in-the-library)
    * [The Runtime class](#the-runtime-class)
    * [The Terminal class](#the-terminal-class)
    * [The Json formatter](#the-json-formatter)
    * [The CsvWriter class](#the-csvwriter-class)
    * [The CsvReader class](#the-csvreader-class)
    * [The JsonClient class](#the-jsonclient-class)
    * [The SmtpClient class](#the-smtpclient-class)
    * [The ObjectMapper component](#the-objectmapper-component)
    * [Network](#the-network)
    * [ObjectComparer](#the-objectcomparer)
    * [DependencyContainer](#the-dependencycontainer)
    * [MessageHub](#the-messagehub)
    * [LdapConnection](#the-ldapconnection)
    * [The ProcessRunner class](#the-processrunner-class)

## Libraries
We offer the Swan library in two flavors since version 0.24. Swan Lite provides basic classes and extension methods and Swan Standard (we call it Fat Swan) provides everything in Swan Lite plus Network, WinServices, DI and more. See the following table to understand the components available to these flavors of Swan.

| Component | Swan Lite | Swan Standard |
|---|---|---|
| [AppWorkerBase](https://unosquare.github.io/swan/api/Unosquare.Swan.Abstractions.AppWorkerBase.html) | :x: | :heavy_check_mark: |
| [ArgumentParser](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ArgumentParser.html) | :heavy_check_mark: | :heavy_check_mark: |
| [ByteArrayExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.ByteArrayExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [CircularBuffer](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.CircularBuffer.html) | :x: | :heavy_check_mark: |
| [Connection](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.Connection.html) | :x: | :heavy_check_mark: |
| [ConnectionListener](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.ConnectionListener.html) | :x: | :heavy_check_mark: |
| [CsProjFile<T>](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.CsProjFile-1.html) | :x: | :heavy_check_mark: |
| [CsvReader](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvReader.html) | :heavy_check_mark: | :heavy_check_mark: |
| [CsvWriter](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvWriter.html) | :heavy_check_mark: | :heavy_check_mark: |
| [DateExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.DateExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [DateTimeSpan](https://unosquare.github.io/swan/api/Unosquare.Swan.DateTimeSpan.html) | :heavy_check_mark: | :heavy_check_mark: |
| [Definitions](https://unosquare.github.io/swan/api/Unosquare.Swan.Definitions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [DependencyContainer](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.DependencyContainer.html) | :x: | :heavy_check_mark: |
| [EnumHelper](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.EnumHelper.html) | :heavy_check_mark: | :heavy_check_mark: |
| [Extensions](https://unosquare.github.io/swan/api/Unosquare.Swan.Extensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [FunctionalExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.FunctionalExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [Json](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.Json.html) | :heavy_check_mark: | :heavy_check_mark: |
| [JsonClient](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.JsonClient.html) | :x: | :heavy_check_mark: |
| [LdapConnection](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.Ldap.LdapConnection.html) | :x: | :heavy_check_mark: |
| [MessageHub](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.MessageHub.html) | :x: | :heavy_check_mark: |
| [Network](https://unosquare.github.io/swan/api/Unosquare.Swan.Network.html) | :x: | :heavy_check_mark: |
| [NetworkExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.NetworkExtensions.html) | :x: | :heavy_check_mark: |
| [ObjectComparer](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectComparer.html) | :heavy_check_mark: | :heavy_check_mark: |
| [ObjectMapper](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectMapper.html) | :heavy_check_mark: | :heavy_check_mark: |
| [ProcessRunner](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ProcessRunner.html) | :x: | :heavy_check_mark: |
| [ReflectionExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.ReflectionExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [Runtime](https://unosquare.github.io/swan/api/Unosquare.Swan.Runtime.html) | :heavy_check_mark: | :heavy_check_mark: |
| [SettingsProvider<T>](https://unosquare.github.io/swan/api/Unosquare.Swan.Abstractions.SettingsProvider-1.html) | :heavy_check_mark: | :heavy_check_mark: |
| [SingletonBase<T>](https://unosquare.github.io/swan/api/Unosquare.Swan.Abstractions.SingletonBase-1.html) | :heavy_check_mark: | :heavy_check_mark: |
| [SmtpClient](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.SmtpClient.html) | :x: | :heavy_check_mark: |
| [SnmpClient](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.SnmpClient.html) | :x: | :heavy_check_mark: |
| [StringExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.StringExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |
| [Terminal](https://unosquare.github.io/swan/api/Unosquare.Swan.Terminal.html) | :heavy_check_mark: | :heavy_check_mark: |
| [TypeCache<T>](https://unosquare.github.io/swan/api/Unosquare.Swan.Reflection.TypeCache-1.html) | :heavy_check_mark: | :heavy_check_mark: |
| [ValueTypeExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.ValueTypeExtensions.html) | :heavy_check_mark: | :heavy_check_mark: |


## Installation:
----------------

Swan Standard Installation:

[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.svg)](https://badge.fury.io/nu/Unosquare.Swan)

```
PM> Install-Package Unosquare.Swan
```

Swan Lite Installation:

[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.Lite.svg)](https://badge.fury.io/nu/Unosquare.Swan.Lite)

```
PM> Install-Package Unosquare.Swan.Lite
```

## What's in the library

In this section, we present the different components that are available in the Swan library. Please keep in mind that everything in the library is opt-in. Swan is completely opt-in. It won't force you to use any of its components, classes or methods.

### The `Runtime` class

`Runtime` provides properties and methods that provide information about the application environment (including Assemblies and OS) and access to singleton instance of other components inside Swan such as `ObjectMapper`.

[Runtime API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Runtime.html)

### The `Terminal` class

Many times, we find ourselves implementing `Console` output code as some `NLog` or `Log4Net` logger or adapter, especially 
when writing console applications, daemons and Windows services or Linux daemons. We also tend to write `Console` code for reading user 
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

It is also very easy to use, it's thread-safe, and it does not require you to learn anything new. In fact, it simplifies logging
messages and displaying `Console` messages by providing `string` extension methods.

[Terminal API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Terminal.html)

#### Example 1: Writing to the Terminal

This only writes messages out to the `TerminalWriters` if there are any available. In practice, we typically **DO NOT** use
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
$"Hello, today is {DateTime.Today}".Debug();
$"Hello, today is {DateTime.Today}".Warn();
$"Hello, today is {DateTime.Today}".Error();
$"Hello, today is {DateTime.Today}".Trace();
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
// Reads a line of text from the console.
var lineResult = Terminal.ReadLine();

// Reads a number from the input. If unable to parse, it returns the default number, in this case (default 0).
var numberResult = Terminal.ReadNumber("Read Number", 0);

// Creates a table prompt where the user can enter an option based on the options dictionary provided.
var promptResult = Terminal.ReadPrompt("Read Promp", options, "A");

// Reads a key from the terminal preventing the key from being echoed.
var keyResult = Terminal.ReadKey("Read Key");
``` 

#### Example 6: Other Useful Functions

Swan's `Terminal` also provides additional methods to accomplish very specific tasks. Given the fact that `Terminal`
is an asynchronous, thread-safe output queue, we might under certain situations require all of the output queue to be written
out to the `Console` before the program exits. For example, when we write a console application that requires its usage
to be fully printed out before the process is terminated. In these scenarios, we use `Terminal.Flush` which blocks
the current thread until the entire output queue becomes empty.

### The `Json` Formatter

You can serialize and deserialize strings and objects using Swan's `Json` Formatter. It's a great way to transform objects to JSON format and vice versa. For example, you need to send information as JSON format to other point of your application and when arrives it's necessary to get back to the object that is going to be used, and thanks to JSON format the data can interchange in a lightweight way.

[Json API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.Json.html)

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

### The `CsvWriter` class

Many projects require the use of CSV files to export and import data. With `CsvWriter` you can easily write objects and data to CSV format. It also provides a useful way to save the data to a file.

[CsvWriter API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvWriter.html)

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

### The `CsvReader` class

When you need to parse data in CSV files you'll always need an easy way to read and load their contents into lists and classes that are usable by your application. Swan provides the `CsvReader` class to read and load CSV files into objects.

[CsvReader API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvReader.html)

#### Example 1: Reading a CSV data string

This is a way to read CSV formatted string.

```csharp
 // The data to be readed
var data = @"Company,OpenPositions,MainTechnology,Revenue
    Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","500" 
    Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","600";

using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
{               
    // The reader of the CSV
    var reader = new CsvReader(stream, true, Encoding.UTF8);
};
```

#### Example 2: Reading a CSV file

From a CSV file, you can read and load the information into a generic list.

```csharp
// The list of object to be written as CSV
var basicObj = new List<BasicJson>();
// This is where the object is save into a file
CsvWriter.SaveRecords<BasicJson>(basicObj, "C:/Users/user/Documents/CsvFile");
// This is how you can load the records of the CSV file
var loadedRecords = CsvReader.LoadRecords<BasicJson>("C:/Users/user/Documents/CsvFile");
``` 

### The `JsonClient` class

Represents a wrapper `HttpClient` with extended methods to use with JSON payloads and bearer tokens authentication.

[JsonClient API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.JsonClient.html)

#### Example 1: Authentication

You can add Authentication to your requests easily.

```csharp
// The Authenticate
var data = JsonClient.Authenticate("https://mywebsite.com/api/token", "admin", "password");
```

#### Example 2: An HTTP GET request

Easy way to HTTP GET using `JsonClient`.

```csharp
// The GET
var data = JsonClient.Get<BasicJson>("https://mywebsite.com/api/data");
```

#### Example 3: An HTTP POST request

Easy way to HTTP POST using `JsonClient`.

```csharp
// The POST
var data = JsonClient.Post<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

#### Example 4: Making a PUT

Easy way to HTTP PUT using `JsonClient`.

```csharp
// The PUT
var data = JsonClient.Put<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

### The `SmtpClient` class

It's a basic SMTP client that can submit messages to an SMTP server. It's very easy to cvonfigure and it provides a very handy way to make send email messages in your application.

[SmtpClient API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.SmtpClient.html)

#### Example 1: Using `System.Net.Mail.MailMessage`
`SmtpClient` uses the classic  `System.Net.Mail.MailMessage` provided by .NET to send emails asynchronously.

```csharp
// Create a new smtp client using google's smtp server
var client = new SmtpClient("smtp.gmail.com", 587);

// Send an email 
client.SendMailAsync(new MailMessage("sender@test.com", "recipient@test.cm", "Subject", "Body"));
```
#### Example 2: Using a SMTP session state
```csharp
// Create a new session state with a sender address
var session = new SmtpSessionState {SenderAddress = "sender@test.com"};

// Add a recipient
session.Recipients.Add("recipient@test.cm");

// Send
client.SendMailAsync(session);

```
#### Example 3: Adding an attatchment with SMTP session state
When using `SmtpSessionState` you have to deal with raw data manipulation, in order to parse MIME attachments [MimeKit](https://www.nuget.org/packages/MimeKit/) is recommended.
```csharp
// Create a new session state with a sender address
var session = new SmtpSessionState { SenderAddress = "sender@test.com" };

// Add a recipient
session.Recipients.Add("recipient@test.cm");

// load a file as an attachment
var attachment = new MimePart("image", "gif")
{
    Content = new MimeContent(File.OpenRead("meme.gif"), ContentEncoding.Default),
    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
    ContentTransferEncoding = ContentEncoding.Base64,
    FileName = Path.GetFileName("meme.gif")
};


using (var memory = new MemoryStream())
{
    //Decode the attachment content
    attachment.Content.DecodeTo(memory);
    
    //Convert it into a byte array and add it to the session DataBuffer
    session.DataBuffer.AddRange(memory.ToArray());
}

// Send
client.SendMailAsync(session);

```
### The `ObjectMapper` component

The `ObjectMapper` is a component to translate and copy property data from one type to another. You can access a default instance of `ObjectMapper` through the `Runtime` class.

[ObjectMapper API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectMapper.html)

#### Example 1: Mapping with default map

The conversion generates a map automatically between the properties in base of the properties names.

```csharp
// Here is mapping the specific user to a destination
var destination = Runtime.ObjectMapper.Map<UserDto>(user);
```

#### Example 2: Mapping with a custom map

With `CreateMap` you generate a new map and you can map one custom property with `MapProperty`.

```csharp
// Creating an Object Mapper
var mapper = new ObjectMapper();
// Creating the map and mapping the property
mapper.CreateMap<User, UserDto>().MapProperty(d => d.Role, s => s.Role.Name);
// Then you map the custom map to a destination
var destination = mapper.Map<UserDto>(user);            
```

#### Example 3: Removing a property from the map

To remove a custom property, you also use `CreateMap` and then remove the custome property of the mapping.

```csharp
// Create an Object Mapper
var mapper = new ObjectMapper();
// Creating a map and removing a property
mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t.Name);
// Then you map the custom map to a destination
var destination = mapper.Map<UserDto>(user);
```

### The `Network`

When you are working with projects related to network or you want to extend your application to use some network functionality the Swan's `Network` provides miscellaneous network utilities such as a Public IP finder, a DNS client to query DNS records of any kind, and an NTP client.

[Network API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Network.html)

#### Example 1: IPv4 and adapters information

It's always useful to have a tool that gives you access to your adapters information and your IP address local and public and use it in your application.

```csharp
// Gets the active IPv4 interfaces.
var interfaces = Network.GetIPv4Interfaces();

// Retrieves the local IP addresses.
var address = Network.GetIPv4Addresses();

// Gets the public IP address using ipify.org.
var publicAddress = Network.GetPublicIPAddress();
```

#### Example 2: DNS and NTP

Also, you can use the `Network` utility to access the IPs of the DNS servers and the UTC from the NTP servers.

```csharp
// Gets the configured IPv4 DNS servers for the active network interfaces.
var dnsServers = Network.GetIPv4DnsServers();

// Gets the DNS host entry (a list of IP addresses) for the domain name.
var dnsAddresses = Network.GetDnsHostEntry("google-public-dns-a.google.com");

// Gets the reverse lookup FQDN of the given IP Address.
var dnsPointer = Network.GetDnsPointerEntry(IPAddress.Parse("8.8.8.8"));

// Queries the DNS server for the specified record type.
var mxRecord = Network.QueryDns("google-public-dns-a.google.com", DnsRecordType.MX);

// Gets the UTC time by querying from an NTP server
var dateTime = Network.GetNetworkTimeUtc();
```

### The `ObjectComparer`

Many times, you need to compare the values inside of an object, array, struct or enum, to do so you need to implement your on code or iterate to find if the values are equals. With `ObjectComparer` you easily compare the properties. It represents a quick object comparer using the public properties of an object or the public members in a structure.

[ObjectComparer API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectComparer.html)

```csharp
// Compare if two variables of the same type are equal.
ObjectComparer.AreEqual(first, second)

// Compare if two objects of the same type are equal. 
ObjectComparer.AreObjectsEqual(first, second);

// Compare if two structures of the same type are equal.
ObjectComparer.AreStructsEqual(first, second)

// Compare if two enumerables are equal.
ObjectComparer.AreEnumsEqual(first, second)
```

### The `DependencyContainer`

It's an easy to use IoC Inversion of Control Container of your classes and interfaces, you can register and associate your class with the interface that is going to be use and then when you finish working with that you can unregister them. You can access a singleton instance of `DependencyContainer` called `Current` by `DependencyContainer` class.

[DependencyContainer API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.DependencyContainer.html)

#### Example 1: Basic Example

```csharp
// Initialize a new instance of DependencyContainer
var container = new DependencyContainer();

// Creates/replaces a named container class registration with a given implementation and default options. 
container.Register<IAnimal, Cat>();

// Attempts to resolve a type using specified options.
var resolve = container.Resolve<IAnimal>();

// Remove a named container class registration.
container.Unregister<IAnimal>();            
```

#### Example 2: Using the DependencyContainer `Current` singleton

```csharp
// Creates/replaces a named container class registration with a given implementation and default options. 
DependencyContainer.Current.Register<IAnimal, Dog>();

// Attempts to resolve a type using specified options.
var resolve = DependencyContainer.Current.Resolve<IAnimal>();

// Remove a named container class registration.
DependencyContainer.Current.Unregister<IAnimal>();    
```

#### Example 3: `CanResolve`

A very handy method to determine if a type can be resolve.

```csharp
// Using CanResolve to check if type can be resolve
if (Runtime.Container.CanResolve<IAnimal>())
{
    // Attempts to resolve a type using specified options.
    Runtime.Container.Resolve<IAnimal>();
}
```

### The `MessageHub`
A simple [Publisher-Subscriber pattern](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) implementation. It's a good alternative when your application requires independent, long-running processes to communicate with each other without the need for events which can make code difficult to write and maintain. 

[MessageHub API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.IMessageHub.html)

In many scenarios you need a way to know when something happens to an object, there are usually two ways of achieving this: constantly checking the object's properties or using the pub-sub pattern. To avoid any problems caused by the former method like possible modification of the object's properties it is a good practice to use the latter. With the pub-sub pattern any object can "subscribe" to another object's event, if the other object "publishes" a message the event is triggered and the custom content of the message is sent. Neither the publisher nor the subscriber knows the existence of one another, therefore the publisher does not directly notify its subscribers, instead there is another component called MessageHub which is known by both(subscriber and publisher) and that filters all incoming messages and distributes them accordingly.

#### Example 1: Subscribing to a MessageHub

A simple example using the DependencyContainer discussed above. Keep in mind that in this example both the subscription and the message sending are done in the same place but this is only for explanatory purposes.

``` csharp
// Using DependencyContainer to create an instance of MessageHub
 var messageHub = DependencyContainer.Current.Resolve<IMessageHub>() as MessageHub;
 
 // Here we create an instance of the publisher class which has a string as its content
 var message = new MessageHubGenericMessage<string>(this, "SWAN");
 
 // Then this object subscribes to the publisher's event and just prints its content which is a string 
 // a token is returned which can be used to unsubscribe later on
 var token = messageHub.Subscribe<MessageHubGenericMessage<string>>(m => m.Content.Info());
 
 // We publish a message and SWAN should be printed on the console
 messageHub.Publish(message);
 
 // And lastly unsuscribe, we will no longer receive any messages 
 MessageHub.Unsubscribe<MessageHubGenericMessage<string>>(token);
``` 

### The `LDAPConnection`
The **Lightweight Directory Access Protocol** or LDAP is a network protocol for querying and modifying items in directory service providers like [Active Directory](https://en.wikipedia.org/wiki/Active_Directory) which provide a systematic set of records organized in a hierarchical structure. Active Directory stores information about users, computers, groups and other objects that are part of a `domain`.

[LdapConnection API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.Ldap.LdapConnection.html)

#### Operations
LDAP has a couple of operations that can be executed


* **Bind**: binds the current connection to a set of  credentials
* **Unbind or Disconnect**: signals the server that the connection is about to close then the server proceeds to close the connection to the client
* **Modify**: this operation is used by LDAP clients to request a change to be performed to the already existing database. This operation is used in combination with one of following :
  * **Add**: inserts a new entry into the directory 
  * **Delete**: deletes an entry from the directory
   * **Replace**: modifies an existing property value
   
#### Example 1: Connecting to a LDAP Server
A connection to a LDAP server is a two step process, first we `connect` to a server but that connection is unauthenticated so we need to bind it to a set of credentials. The reason for breaking down the connection process into a two step action allows us to reset the authorization state using the same connection. 

```csharp
 // Create a  LdapConnection variable
 var connection = new LdapConnection();
 
 // Connect to a server with a deafult port 
 await connection.Connect("ldap.forumsys.com", 389);
 
 // Set up the credentials 
 await connection.Bind("cn=read-only-admin,dc=example,dc=com", "password");
```
#### Example 2: Reading all the properties of an entry
After establishing a connection you can use the connection's Read method to retrieve all properties of an entry
```csharp
// Get all properties of 'tesla'
 var properties = await connection.Read("uid=tesla,dc=example,dc=com");
 
 // After getting all properties from an entry select its email and print it
 properties.GetAttribute("mail").StringValue.Info();
```
#### Example 3: Searching entries
 There are three scopes for searching entries :
1. **ScopeBase**: searches only at the base dn
2. **ScopeOne**: searches all entries one level under the specified dn
3. **ScopeSub**: as mentioned above this allows to search entries at all levels

```csharp
// Retrieve all entries that have the specified email using ScopeSub 
// which searches all entries at all levels under and including the specified base DN
var searchResult = await connection.Search("dc=example,dc=com",LdapConnection.ScopeSub,"(cn=Isaac Newton)");

// If there are more entries remaining
  while (searchResult.HasMore())
  {
      // Point to the next entry
      var entry = searchResult.Next();
      
      // Get all attributes 
      var entryAttributes = entry.GetAttributeSet();
      
      // Select its email and print it
      entryAttributes.GetAttribute("cn").StringValue.Info();
  }
```
 #### Example 4: Modifying an entry attribute 
An easy way to deal with attributes modification is by calling the Modify method with a `LdapModificationOp` such as:
* **Replace**: overrides an attribute value. 
   * If the attribute does not exist it creates a new one
   * If no value is passed the entire attribute is deleted
* **Delete** : deletes a value from an attribute.
   * If no values are listed or if all of them are the entire attribute is removed
* **Add**: adds a new value to an attribute 
   * If the attribute does not exist a new one is created
 ```csharp
 // Modify Tesla and sets its email as tesla@email.com
 connection.Modify("uid=tesla,dc=example,dc=com", 
    new[] { new LdapModification(LdapModificationOp.Replace, "mail", "tesla@email.com") });
    
   // Deletes the listed values from the given attribute
 connection.Modify("uid=tesla,dc=example,dc=com", 
    new[] { new LdapModification(LdapModificationOp.Delete, "mail", "tesla@email.com") });

 // Add back the recently deleted property
 connection.Modify("uid=tesla,dc=example,dc=com", 
    new[] { new LdapModification(LdapModificationOp.Add, "mail", "tesla@email.com") });


 // disconnect from the LDAP server
 connection.Disconnect();
 ```
### The `ProcessRunner` class
A class that provides methods that helps us create external process and capture their output. 

[ProcessRunner API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ProcessRunner.html)

#### Example 1: Running a process async
`RunProcessAsync` runs an external process asynchronously and returns the exit code. It provides error and success callbacks to capture binary data from the ouput and error stream.

```csharp
// executes a process and returns the exit code
var result = await ProcessRunner.RunProcessAsync(
               // The path of the program to be executed
               "dotnet",
               //Parameters
               "--help",
               // A success callback with a reference to the output and the process itself
               (data, proc) =>
               {
               // If it executes correctly, print the output
                 Encoding.GetEncoding(0).GetString(data).WriteLine();
               },
               // An error callback with a reference to the error and the process itself
               (data, proc) =>
               {
               // If an error ocurred, print out the error
                   Encoding.GetEncoding(0).GetString(data).WriteLine();
               }
              );
 ```
#### Example 2: Getting a process output
If you are more concern about the output than the process itself, you can use `GetProcessOutputAsync` to get just a string containing either the output or the error text.
```csharp
// Execute a process asynchronously and return either the ouput or the error
var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "--help");

// Print the result
data.WriteLine();
 ```
#### Example 3: Getting a process result
If you don't want to deal with callbacks but you need more information after running an external process, you can use `GetProcessResultAsync` to get not just the output and error texts but also the exit code.
```csharp
// Execute a process asynchronously and returns a ProcessResult object
var data = await ProcessRunner.GetProcessResultAsync("dotnet", "--help");

// Print out the exit code
$"{data.ExitCode}".WriteLine();

// The output
data.StandardOutput.WriteLine();

// And the error
data.StandardError.WriteLine();
```
*Keep in mind that both `GetProcessOutputAsync` and `GetProcessResultAsync` are meant to be used for programs that output a relatively small amount of text*

### The `AppWorkerBase` class
An implementation of the IWorker interface that creates an application service capable of performing some background processing.

#### Example 1: Inherit from AppWorkerBase
The AppWorkerBase has many methods that can be overwritten such as:

* **OnWorkerThreadLoopException**: which is called when an unhandled exception is thrown
* **OnWorkerThreadExit**: executed when the user loop has exited
* **WorkerThreadLoop**: a custom loop that checks wheter a cancellation has been requested if so it exits the loop

```csharp
 class Worker : AppWorkerBase
    {
        // An action that will be executed if the worker is stopped
        public Action OnExit { get; set; }
        
        // Override the base loop method, this is the code that'll be run once the worker is started
        protected override void WorkerThreadLoop()
        {
            // While the worker hasn't been stopped
            while (CancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    // Delay a second and then proceed
                    Task.Delay(TimeSpan.FromMilliseconds(1000), CancellationToken).Wait();                    
                    
                    // Just print this
                    $"Working...".WriteLine();
                }
                catch
                {
                }
            }
           
        }
        
        // Once the worker is stopped this code will be executed
        protected override void OnWorkerThreadExit()
        {          
            // Execute the base method
            base.OnWorkerThreadExit();
            
            // Then if the OnExit Action is not null execute it
            OnExit?.Invoke();
        }
    }
```
#### Example 2: Using an AppWorker
The Worker's base class also includes properties like:
* **isBusy**: indicates if the thread is busy
* **State**: shows the current state of the app service, based on the AppWorkerState enum (Stopped, Running)
* **CancellationToken**: gets the cancellation token
```csharp
// Create a new AppWorker using the class explained above
var worker = new Worker
            {
            // Setting an OnExit Action that just prints 'Exited'
                OnExit = () =>
                {
                    $"Exited".WriteLine();
                }
            };
// Start the worker
worker.Start();

// Wait 2 seconds in order to see some output
Task.Delay(2000).Wait();

// Stop the worker         
worker.Stop();
```
