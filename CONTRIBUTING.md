# ElezenTools Contribution Guide

Hello, and thanks for your interest! 

ElezenTools is designed to be a beginner friendly, opinionated, and stable library
to help newcomers to XIV modding find their footing and get creating easily. It
also aims to solve the problem of compatibility breaks between API versions by providing
shims as needed, so that developers who are taking a break from the game don't need
to risk having their plugin marked as abandoned. It also aims to solve the problem of
many developers fixing their own plugins and writing near-identical code by providing a 
central location for those who want to use it.

## How you can contribute
- Contribute code and services (bug fixes, new features, refactoring)
- Help tackle breakages after a Dalamud API update
- Suggest features
- Improve documentation
- Build examples
- Help test!

## Before You Begin

Before writing code for ElezenTools, it's important to remember the audience we're targeting.
Historically, modding has been an activity that acts as a gateway for non-programmers to
whet their appetite and learn a few things. The guidelines below are written with an 
aim to make ElezenTools easy to use for complete beginners. The intention is that they're
able to quickly use this to get themselves started and get into the mindset of how things work -
both with programming in general, an XIV modding specifically. If, later on, they want to move towards using
Dalamud directly, that pathway is left open for them by studying ElezenTools' code.

We want more people making mods, not fewer! 

## Technical Guidelines

ElezenTools uses Meziantou and Sonar for code analysis. We should aim for zero 
warnings without a good reason - if there are warnings, a commit or PR should explain
why they need to be left in. 

Generally speaking, we prefer static methods wherever possible so that a user can call
something and have it returned for them to deal with - for example, `ElezenData.Jobs` provides
methods to return a dictionary of all Jobs, or find them by a specific attribute.

When dealing with Lumina, we provide the user with a struct containing anything they might
want - for example. `ElezenData.Jobs` returns a dictionary of `JobData` structs. 

Code-wise, prefer verbosity over brevity. It's expected that newcomers will eventually
get curious how something is done and inspect our code - we want it to be easy to understand
to those who are less comfortable.

When submitting a pull request, try and include example usage - this'll end up in the docs, 
and makes slapping together a test plugin easier if needed. 

If a PR can't easily be felt out by reading it, it'll be merged after testing directly in game.

## Setting up development

ElezenTools has a hard dependency on Dalamud only. The .csproj is already set up to look
for it in the `.xlcore` and `%AppData%` folders, so Linux and Windows users can get building 
immediately.

Any IDE that supports C# can be used. 

## Testing and Quality Control

As a rule, any functions added should have a clear, well-documented explanation of what it
does, what its parameters are, and what it returns. Even better if it has an example usage. 

If it's something that has a visual impact on screen, or can be tested easily, attaching screenshots
to the PR is most welcome. Otherwise, please include testing instructions in the PR so that it can be
checked. 

Eventually, we'll be using Dalamock to automate some of this.

## Licensing and Legal

ElezenTools is licensed under AGPL v3 or later, and as such, all contributions will also
be licensed as such. 

AGPL is a strong copyleft license. This means that if a user creates a mod that uses
ElezenTools, their mod must also be released under the AGPL and its source code
must be made available.