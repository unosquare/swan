[![NuGet](https://img.shields.io/nuget/dt/Unosquare.Swan.svg)](https://www.nuget.org/packages/Unosquare.Swan)
[![Build Status](https://travis-ci.org/unosquare/swan.svg?branch=master)](https://travis-ci.org/unosquare/swan)
[![Build status](https://ci.appveyor.com/api/projects/status/063tybvog8mb1sic/branch/master?svg=true)](https://ci.appveyor.com/project/geoperez/swan/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/unosquare/swan/badge.svg?branch=master)](https://coveralls.io/github/unosquare/swan?branch=master)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/c588287f33694935a4d061e82baf62f5)](https://www.codacy.com/project/UnosquareLabs/swan/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=unosquare/swan&amp;utm_campaign=Badge_Grade_Dashboard)
[![Dependabot Status](https://api.dependabot.com/badges/status?host=github&repo=unosquare/swan)](https://dependabot.com)

# <img src="https://github.com/unosquare/swan/raw/master/swan-logo-32.png"></img> SWAN: Stuff We All Need (Unosquare's collection of C# extension methods and classes)

*:star: Please star this project if you find it useful!*

SWAN stands for Stuff We All Need

Repeating code and reinventing the wheel is generally considered bad practice. At [Unosquare](https://www.unosquare.com) we are committed to beautiful code and great software. Swan is a collection of classes and extension methods that we (and other good developers) have written and evolved over the years. We found ourselves copying and pasting the same code for every project every time we started them. We decided to kill that cycle once and for all. This is the result of that idea. Our philosophy is that Swan should have no external dependencies, it should be cross-platform, and it should be useful.

Table of contents
=================

  * [📚 Libraries](#-libraries)
  * [💾Installation](#-installation)
  * [What's in the library](#whats-in-the-library)
    * [The Runtime component](#the-runtime-component)
    * [The Terminal class](#the-terminal-class)
    * [The Json formatter](#the-json-formatter)
    * [The CsvWriter class](#the-csvwriter-class)
    * [The CsvReader class](#the-csvreader-class)
    * [The JsonClient class](#the-jsonclient-class)
    * [The SmtpClient class](#the-smtpclient-class)
    * [The ObjectMapper component](#the-objectmapper-component)
    * [The Network component](#the-network-component)
    * [The ObjectComparer component](#the-objectcomparer-component)
    * [The ObjectValidator component](#the-objectvalidator-component)
    * [The DependencyContainer component](#the-dependencycontainer-component)
    * [The MessageHub component](#the-messagehub-component)
    * [The LdapConnection class](#the-ldapconnection-class)
    * [The ProcessRunner class](#the-processrunner-class)
    * [The ArgumentParser component](#the-argumentparser-component)
    * [The SettingsProvider abstraction](#the-settingsprovider-abstraction)
    * [The Connection class](#the-connection-class)
    * [The Benchmark component](#the-benchmark-component)
    * [The DelayProvider component](#the-delayprovider-component)
    * [The WaitEventFactory component](#the-waiteventfactory-component)
    * [Atomic Types](#atomic-types)
  * [Running Unit Tests](#running-unit-tests)

## 📚 Libraries
We offer the Swan library in two flavors since version 0.24. Swan Lite provides basic classes and extension methods and Swan Standard (we call it Fat Swan) provides everything in Swan Lite plus Network, WinServices, DI and more. See the following table to understand the components available to these flavors of Swan.

| Component | Swan Lite | Swan Standard |
|---|---|---|
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
| [MessageHub](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.MessageHub.html) | :x: | :heavy_check_mark: |
| [Network](https://unosquare.github.io/swan/api/Unosquare.Swan.Network.html) | :x: | :heavy_check_mark: |
| [NetworkExtensions](https://unosquare.github.io/swan/api/Unosquare.Swan.NetworkExtensions.html) | :x: | :heavy_check_mark: |
| [ObjectComparer](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectComparer.html) | :heavy_check_mark: | :heavy_check_mark: |
| [ObjectMapper](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectMapper.html) | :heavy_check_mark: | :heavy_check_mark: |
 | [ObjectValidator](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectValidator.html) | :heavy_check_mark: | :heavy_check_mark: |
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
| [WorkerBase](https://unosquare.github.io/swan/api/Unosquare.Swan.Abstractions.WorkerBase.html) | :x: | :heavy_check_mark: |
 
If you are developing an ASP.NET Core application, we recommend to use [SWAN AspNet.Core](https://github.com/unosquare/swan-aspnetcore).

## 💾 Installation:

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

### The `Runtime` component

`Runtime` provides properties and methods that provide information about the application environment (including Assemblies and OS) and access to singleton instance of other components inside Swan such as `ObjectMapper`.

[Runtime API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Runtime.html)

### The `Terminal` class

Many times, we find ourselves implementing `Console` output code as some `NLog` or `Log4Net` logger or adapter, especially when writing console applications, daemons, and Windows services or Linux daemons. We also tend to write `Console` code for reading user input because it can't be some logger or adapter. And then you have the `System.Diagnostics.Debug` class to write to the debugger output. And finally, all your `Console` user interaction looks primitive and unprofessional. In other 
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

This is where `Terminal` really shines. Instead of using the `Write` and `WriteLine` methods, you can use the methods that are intended for logging. These methods have different purposes and distinct functionality. Please
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
is an asynchronous, thread-safe output queue, we might under certain situations require all of the output queues to be written
out to the `Console` before the program exits. For example, when we write a console application that requires its usage
to be fully printed out before the process is terminated. In these scenarios, we use `Terminal.Flush` which blocks
the current thread until the entire output queue becomes empty.

### The `Json` Formatter

You can serialize and deserialize strings and objects using Swan's `Json` Formatter. It's a great way to transform objects to JSON format and vice versa. For example, you need to send information as JSON format to another point of your application and when arrives it's necessary to get back to the object that is going to be used, and thanks to JSON format the data can interchange in a lightweight way.

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
#### Example 4: Serialize an object using attributes

Serializes the specified object whose properties have a `JsonPropertyAttribute`
```csharp
 class JsonPropertyExample
{
   [JsonProperty("data")]
   public string Data { get; set; }
   
   [JsonProperty("ignoredData", true)]
   public string IgnoredData { get; set; }
}
```
```csharp
 var obj = new JsonPropertyExample() { Data = "OK", IgnoredData = "OK" };
 
 // {"data": "OK"}
 var serializedObj = Json.Serialize(obj);
```
#### Example 5: Deserialize

Deserializes the specified JSON `string` as either a `Dictionary<string, object>` or as a `List<object>` depending on the syntax of the JSON `string`.

```csharp 
// The json to be deserialize
var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
// Deserializes the specified json into Dictionary<string, object>.
var data = Json.Deserialize(basicJson);
``` 

#### Example 6: Deserialize a generic type `<T>`

Deserializes the specified JSON `string` and converts it to the specified object type. Non-public constructors and property setters are ignored.

```csharp 
// The json Type BasicJson to be deserialize
var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
// Deserializes the specified string in a new instance of the type BasicJson.
var data = Json.Deserialize<BasicJson>(basicJson);
``` 

### The `CsvWriter` class

Many projects require the use of CSV files to export and import data. With `CsvWriter` you can easily write objects and data to CSV format. It also provides a useful way to save data into a file.

[CsvWriter API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvWriter.html)

#### Example 1: Writing a List of objects

This is the way to write a list of objects into a CSV format.

```csharp
 // The list of objects to be written as CSV
var basicObj = new List<BasicJson>();

using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(basicObj.ToString())))
{               
    // The CSV writer
    var reader = new CsvWriter(stream);
};
```

#### Example 2: Writing a List of objects into a file

You also can write the object into a file or a temporal file.

```csharp
// The list of objects to be written as CSV
var basicObj = new List<BasicJson>();
// This is where the object is save into a file
CsvWriter.SaveRecords(basicObj, "C:/Users/user/Documents/CsvFile");
```

### The `CsvReader` class

When you need to parse data in CSV files you'll always need an easy way to read and load their contents into lists and classes that are usable by your application. Swan provides the `CsvReader` class to read and load CSV files into objects.

[CsvReader API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Formatters.CsvReader.html)

#### Example 1: Reading a CSV data string

This is a way to read CSV formatted string.

```csharp
 // The data to be read
var data = @"Company,OpenPositions,MainTechnology,Revenue
            Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"",500 
            Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"",600";

using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
{               
    // The CSV reader
    var reader = new CsvReader(stream, true, Encoding.UTF8);
};
```

#### Example 2: Reading a CSV file

From a CSV file, you can read and load the information into a generic list.

```csharp
// The list of object to be written as CSV
var basicObj = new List<BasicJson>();
// This is where the object is save into a file
CsvWriter.SaveRecords(basicObj, "C:/Users/user/Documents/CsvFile");
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

An easy way to HTTP GET using `JsonClient`.

```csharp
// The GET
var data = JsonClient.Get<BasicJson>("https://mywebsite.com/api/data");
```

#### Example 3: An HTTP POST request

An easy way to HTTP POST using `JsonClient`.

```csharp
// The POST
var data = JsonClient.Post<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

#### Example 4: Making a PUT

An easy way to HTTP PUT using `JsonClient`.

```csharp
// The PUT
var data = JsonClient.Put<BasicJson>("https://mywebsite.com/api/data", new { filter = true });
```

### The `SmtpClient` class

It's a basic SMTP client that can submit messages to an SMTP server. It's very easy to configure and it provides a very handy way to make send email messages in your application.

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
#### Example 3: Adding an attachment with SMTP session state
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

The conversion generates a map automatically between the properties in the base of the properties names.

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

To remove a custom property, you also use `CreateMap` and then remove the custom property of the mapping.

```csharp
// Create an Object Mapper
var mapper = new ObjectMapper();
// Creating a map and removing a property
mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t.Name);
// Then you map the custom map to a destination
var destination = mapper.Map<UserDto>(user);
```

### The `Network` component

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

### The `ObjectComparer` component

Many times, you need to compare the values inside of an object, array, struct or enum, to do so you need to implement your own code or iterate to find if the values are equals. With `ObjectComparer` you easily compare the properties. It represents a quick object comparer using the public properties of an object or the public members in a structure.

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
### The `ObjectValidator` component
A simple object validator that allows you to set custom validations and identify if an object satisfies them.

[ObjectValidator API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectValidator.html)

[ObjectValdiationResult API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ObjectValidationResult.html)

### Example 1: Simple object validation
Our `Simple` class to validate
```csharp
  public class Simple
    {
        public string Name { get; set; }
    }
```

```csharp
// create an instance of ObjectValidator
var obj = new ObjectValidator();

// Add a validation to the 'Simple' class with a custom error message
obj.AddValidator<Simple>(x => !string.IsNullOrEmpty(x.Name), "Name must not be empty");

// validate and return a boolean
var res = obj.IsValid(new Simple { Name = "Name" });
```

### Example 2: Using Attributes
Both `IsValid` and `Validate` methods verify that the object satisfies all custom validators and/or attributes, but instead of just returning a boolean, `Validate` returns a `ObjectValidatorResult` which includes all the errors with their properties.

Our `Simple` class to validate
```csharp
  public class Simple
    {
        [NotNull]
        public string Name { get; set; }
        
        [Range(1, 10)]
        public int Number { get; set; }
        
        [Email]
        public string Email { get; set; }
    }
```
This time we'll be using both custom validators and attributes 

```csharp
// using the Runtime's ObjectValidator singleton
Runtime.ObjectValidator.AddValidator<Simple>(x => !x.Name.Equals("Name"), "Name must not be 'Name'");
 
var res =  Runtime.ObjectValidator.Validate(new Simple{ Name = "name", Number = 5, Email ="email@mail.com"})

```

### Example 3: Using the extension method
In this example, we'll use the previous `Sample` class to validate an object using the built-in extension method which in turn uses the `Runtime`'s `ObjectValidator` singleton to validate our object.

```csharp
// using the Runtime's ObjectValidator singleton
Runtime.ObjectValidator.AddValidator<Simple>(x => !x.Name.Equals("Name"), "Name must not be 'Name'");

// using the extension method
var res = new Simple{ Name = "name", Number = 5, Email ="email@mail.com"}.IsValid();

```
### The `DependencyContainer` component

It's an easy to use IoC Inversion of Control Container of your classes and interfaces, you can register and associate your class with the interface that is going to use and then when you finish working with that you can unregister them. You can access a singleton instance of `DependencyContainer` called `Current` by `DependencyContainer` class.

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

A very handy method to determine if a type can be resolved.

```csharp
// Using CanResolve to check if type can be resolve
if (Runtime.Container.CanResolve<IAnimal>())
{
    // Attempts to resolve a type using specified options.
    Runtime.Container.Resolve<IAnimal>();
}
```

### The `MessageHub` component
A simple [Publisher-Subscriber pattern](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) implementation. It's a good alternative when your application requires independent, long-running processes to communicate with each other without the need for events which can make code difficult to write and maintain. 

[MessageHub API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.IMessageHub.html)

In many scenarios you need a way to know when something happens to an object, there are usually two ways of achieving this: constantly checking the object's properties or using the pub-sub pattern. To avoid any problems caused by the former method like a possible modification of the object's properties it is a good practice to use the latter. With the pub-sub pattern, any object can "subscribe" to the publisher's publish event. When a message is "published" the event is triggered and the custom content of the message is sent. Neither the publisher nor the subscriber knows the existence of one another, therefore the publisher does not directly notify its subscribers, instead there is another component called MessageHub which is known by both(subscriber and publisher) and that filters all incoming messages and distributes them accordingly.

#### Example 1: Subscribing to a MessageHub

A simple example using the DependencyContainer discussed above. Keep in mind that in this example both the subscription and the message sending are done in the same place but this is only for explanatory purposes.

``` csharp
// use DependencyContainer to create an instance of MessageHub
 var messageHub = DependencyContainer.Current.Resolve<IMessageHub>() as MessageHub;
 
 // create an instance of the publisher class which has a string as its content
 var message = new MessageHubGenericMessage<string>(this, "SWAN");
 
 // subscribe to the publisher's event and just print its content which is a string 
 // a token is returned which can be used to unsubscribe later on
 var token = messageHub.Subscribe<MessageHubGenericMessage<string>>(m => m.Content.Info());
 
 //publish a message and SWAN should be printed on the console
 messageHub.Publish(message);
 
 // unsuscribe, we will no longer receive any messages 
 messageHub.Unsubscribe<MessageHubGenericMessage<string>>(token);
``` 

### The `LDAPConnection` class

The LDAP Client was moved to a standalone assembly at [SWAN LDAP](https://github.com/unosquare/swan-ldap).

### The `ProcessRunner` class
A class that provides methods that helps us create external processes and capture their output. 

[ProcessRunner API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ProcessRunner.html)

#### Example 1: Running a process async
`RunProcessAsync` runs an external process asynchronously and returns the exit code. It provides error and success callbacks to capture binary data from the output and error stream.

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

### The `ArgumentParser` component

This component allows us to parse command line arguments and reconstruct those values into an object, making them much easier to manipulate.

[ArgumentParser API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.ArgumentParser.html)

#### Example 1: Using basic options

In order to parse arguments first, we need to create a class which the arguments will be parsed into using the `ArgumentOption` attribute.

In order to set an `ArgumentOption`, we need to supply at least a short name, a long name or both

```csharp
  internal class Options
    {
        // This attribute maps a command line option to a property 
        // with 'v' as its short name and 'verbose' as its long name
        [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }
       
        [ArgumentOption('u', Required = true, HelpText = "Set user name.")]
        public string Username { get; set; }
    }
```

When a program is executed using a command line shell, the OS usually allows passing additional information provided along the program name. For instance `example.exe -u user` will execute `example.exe` and the additional text will be passed to it, making the additional arguments accessible to the program using the `args` parameter in the *Main* method.

```csharp
// the variable args contains all the additional information(arguments)
// that were passed during the execution
static void Main(string[] args)
  {
    // create a new instance of the class that we want to parse the arguments into
    var options = new Options();

    // if everything went out fine the ParseArguments method will return true
    Runtime.ArgumentParser.ParseArguments(args, options);

  }
```

#### Example 2: Using an Array
In here the complete argument string will be split into an array using the separator provided.

```csharp
internal class Options
  {   
      [ArgumentOption('n', "names", Separator=',', 
      Required = true, HelpText = "A list of names separated by a comma")]
      public string Names[] { get; set; }
  }
```

#### Example 3: Using an Enum
This maps the argument `--color` to an `Enum` which accepts any of the colors defined in `ConsoleColor` and sets `Red` as the default value.

```csharp
internal class Options
  {        
      [ArgumentOption("color", DefaultValue = ConsoleColor.Red, HelpText = "Set a color.")]
      public ConsoleColor Color { get; set; }
  }
```
### The `SettingsProvider` abstraction
It represents a provider that helps you save and load settings using plain JSON file.

[SettingsProvider API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Abstractions.SettingsProvider-1.html)

#### Example 1: Loading and saving settings
Here we define a `Settings` class that contains all the properties we want.

```csharp
internal class Settings 
    {
       public int Port { get; set; } = 9696;

       public string User { get; set; } = "User";    
    }
```

Once we define our settings we can access them using the `Global` property inside `Instance`.
```csharp
//Get user from settings
var user = SettingsProvider<Settings>.Instance.Global.User;

 //Modify the port 
 SettingsProvider<Settings>.Instance.Global.Port = 20;
 
 //if we want those settings to persist
 SettingsProvider<Settings>.Instance.PersistGlobalSettings();
```

### The `Connection` class
It represents a wrapper for TcpClient (a TCP network connection) either on the server or on the client. It provides access to the input and output network streams. It is capable of working in 2 modes.

[Connection API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.Connection.html)

[ConnectionListener API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Networking.ConnectionListener.html)

#### Example 1: Creating an TCP server

When dealing with a connection on the server side, continuous reading must be enabled, thus deactivating Read methods. If these methods are used an invalid operation exception will be thrown. This example uses a `ConnectionListener` which is a TCP listener manager with built-in events and asynchronous functionality.

```csharp
// create a new connection listener on a specific port
var connectionListener = new ConnectionListener(1337);

// handle the OnConnectionAccepting event
connectionListener.OnConnectionAccepted += (s, e) =>
{
// create a new connection with a blocksize of 6
    using (var con = new Connection(e.Client,6))
    {
      // an event which will be raised when data is received
        con.DataReceived += (o, y) =>
        {
            var response = Encoding.UTF8.GetChars(y.Buffer);
        };

      con.WriteLineAsync("world!").Wait();
    }                
};
connectionListener.Start();
```

#### Example 2: Creating an TCP client
Continuous  reading is usually used on the server side so, you may want to disable them on the client side.

```csharp
// create a new TcpCLient object
var client = new TcpClient();

// connect to a specific address and port
client.Connect("localhost",1337);

//create a new connection with specific encoding, new line sequence and continous reading disabled
using (var cn = new Connection(client, Encoding.UTF8, "\r\n", true, 0))
{
     await cn.WriteDataAsync(Encoding.UTF8.GetBytes("Hello "), true);
     var response = await cn.ReadTextAsync();
}
```

### The `Benchmark` component
A simple benchmarking class used as an `IDisposable` that provides useful statistics about a certain piece of code.


[Benchmark API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.Benchmark.html)

#### Example 1: A simple benchmark test
```csharp
//starts a test with a custom name identifier
using (Benchmark.Start("Test")) 
{

  // do some logic in here
  
}

// dump results into a string
var results = Benchmark.Dump();
```
### The `DelayProvider` component
A useful component that implements several delay mechanisms.

[DelayProvider API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.DelayProvider.html)

#### Example 1: Creating a delay
```csharp
// using the ThreadSleep strategy
using (var delay = new DelayProvider(DelayProvider.DelayStrategy.ThreadSleep))
 {
     // retrieve how much time we delayed
     var time = delay.WaitOne();
 }  
```

### The `WaitEventFactory` component
`WaitEventFactory` provides a standard [ManualResetEvent](https://docs.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=netframework-4.7.1) factory with a unified API. 
`ManualResetEvent` is a variation of `AutoResetEvent` that doesn't automatically reset after a thread is let through on a `WaitOne` call. Calling `Set` on a `ManualResetEvent` serves like an open gate allowing any number of threads that `WaitOne` pass throughCalling and `Reset` closes this gate. This type of event is usually used to signal that a certain operation has completed.

[WaitEventFactory API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.Components.WaitEventFactory.html)

#### Example 1: Using the `WaitEventFactory`

```csharp
// creates a WaitEvent using the slim version of ManualResetEvent
private static readonly IWaitEvent waitEvent = WaitEventFactory.CreateSlim(false);

static void Main()
{
 // start two tasks
    Task.Factory.StartNew(() =>
    {
        Work(1);
    });

    Task.Factory.StartNew(() =>
    {
        Work(2);
    });

    //Send first signal to retrieve data
    waitEvent.Complete();
    waitEvent.Begin();

    Thread.Sleep(TimeSpan.FromSeconds(2));

    // Send second signal
    waitEvent.Complete();

    Console.ReadLine();
}
```
```csharp
static void Work(int taskNumber)
 {
     $"Data retrieved:{taskNumber}".WriteLine();
     waitEvent.Wait();

     Thread.Sleep(TimeSpan.FromSeconds(2));
     $"All finished up {taskNumber}".WriteLine();
 }
```


### Atomic types

Atomic operations are indivisible which means that they cannot interrupted partway through. `SWAN` provides Atomic types which include mechanisms to perform these kinds of operations on Built-In types like: `bool`, `long`, and `double`. This is quite useful in situations where we have to deal with lots of threads performing writes on variables because we can assure that threads will not interrupt each other in the middle of an operation and perform a `torn write`.

[AtomicBoolean API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.AtomicBoolean.html)

[AtomicLong API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.AtomicLong.html)

[AtomicDouble API Doc](https://unosquare.github.io/swan/api/Unosquare.Swan.AtomicDouble.html)

## Running Unit Tests

If you want to run the .NET Unit test project, you may need to start some services. These services are Javascript files and you need [NodeJS](https://nodejs.org/en/download/) to execute them. I know why Javascript files for a .NET project, but it's the easy way to start up some network services, anyway PR with .NET Core services are welcome.

Before running them, please execute `npm install`. This command will install all the required dependencies to start the network services.

The following files, located in the root folder, should be run in any order before start running unit tests:

* `./mail.js` - This script will mount a SMTP server, this service is required to run `SmtpClient` tests.
* `./web.js` -  This script will provide a web server responding JSON files for `JsonClient` tests.
* `./tcp.js` - This script will open a basic TCP Socket for `TcpConnection` tests.
* `./ntp.js` - This script will mount a NTP server for general `Network` methods.

You can also check the CI files ([Travis](https://github.com/unosquare/swan/blob/master/.travis.yml) and [AppVeyor](https://github.com/unosquare/swan/blob/master/appveyor.yml) for details how to run them.
