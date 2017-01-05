[![Build Status](https://travis-ci.org/unosquare/swan.svg?branch=master)](https://travis-ci.org/unosquare/swan)
[![Build status](https://ci.appveyor.com/api/projects/status/063tybvog8mb1sic/branch/master?svg=true)](https://ci.appveyor.com/project/geoperez/swan/branch/master)
[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.svg)](https://badge.fury.io/nu/Unosquare.Swan)
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
```
PM> Install-Package Unosquare.Swan
```

## What's in the library

In this section we present the different components that are available in the Swan library. Please keep in mind that everything in the library is opt-in.
Swan won't force you to use any of its components, classes or methods.

### The Terminal

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

#### Example 1: Writing to the Terminal

This only writes messages out to the `TerminalWriters` if they are avialble. In practice, we typically **DO NOT** use
the `Write` and `WriteLine` methods

```csharp
// The simplest way of writing a line of text:
Terminal.WriteLine($"Hello, today is {DateTime.Today}");

// A slightly better way:
$"Hello, today is {DateTime.Today}".WriteLine();

// Now, add some color:
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green);

// Write it out to the debugger as well!
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);

// You could have also set the color argument to null and just use the default
$"Hello, today is {DateTime.Today}".WriteLine(null, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);
```

#### Example 2: Basic Logging

This is where `Terminal` really shines. It provides a confugrable 