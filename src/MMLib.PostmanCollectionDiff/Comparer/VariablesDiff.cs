using System.Collections;

namespace MMLib.PostmanCollectionDiff.Comparer;

public class VariablesDiff : IDiff, IEnumerable<VariableDiff>
{
    public bool HasDifferences { get; private set; }

    public IEnumerable<VariableDiff> Variables { get; private set; } = new List<VariableDiff>();

    internal void AddVariables(IEnumerable<VariableDiff> variables)
    {
        Variables = variables;
        if (variables.Any(v => v.HasDifferences))
        {
            HasDifferences = true;
        }
    }

    public IEnumerator<VariableDiff> GetEnumerator() => Variables.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Variables.GetEnumerator();
}
