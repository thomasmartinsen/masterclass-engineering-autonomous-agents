// The engineering agent will replace this tiny starter in Demo 2.
var caseDescription = args.Length > 0
    ? string.Join(' ', args)
    : "C-101: Employee cannot sign in; callback number missing.";
Console.WriteLine($"Case input: {caseDescription}");
