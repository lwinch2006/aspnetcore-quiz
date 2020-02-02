# Backend Candidate Challenge

This is a simple challenge to be used as part of an interview process for .NET backend developers. The challenge is intended to be done offline at your own speed using the tools of your choice. There is no single correct solution, just a set of tasks to trigger some relevant discussions. Be prepared to present your solution when we meet.

## Rules
1. You are free to use any tool you like when developing.
2. You can use as much time as you like, but the intention is not to have you spend more than a couple of hours.
3. You are free to pull in any framework or libraries in order to solve the challenge, but be prepared to reason about your choices.
4. Use the patterns and practices you think are best on any part of the challenge, again be prepared to reason about your choices.
5. Commit your changes as you solve the various challenges and whenever you feel is a good time to commit. Let the commit message describe what part you solved.
6. Once you are done, zip down the whole solution and send it back to the person that sent you the challenge.

## Background information
The code is a quiz solution based on .NET Core. You have a quiz client project and a quiz service project, both have corresponding test projects.

The solution uses a SQLite in-memory database with the following schema:
```
Quiz                           Question                        Answer
+-----------------+            +-----------------+            +-----------------+
| Id              |            | Id              |            | Id              |
| Title           |            | Text            |            | Text            |
|                 +------------+ QuizId          +------------+ QuestionId      |
|                 |            | CorrectAnswerId |            |                 |
|                 |            |                 |            |                 |
+-----------------+            +-----------------+            +-----------------+
```

## Challenge tasks
1. Create a Git repository
2. Build the solution and make sure all tests pass before you make any changes.
3. Refactor the Get method in the QuizController to an architecture of your choice making the controller easier to test and maintain.
4. Create a new test where you create a quiz with minimum two questions as testdata for the test, take the quiz and assert that you have the correct score based on number of correct answers. 1 point per correct answer.
5. Do a code review and add TODO comments in the code where you see things that you would fix or do differently e.g.  //TODO I prefer x over z

That's it - best of luck!
