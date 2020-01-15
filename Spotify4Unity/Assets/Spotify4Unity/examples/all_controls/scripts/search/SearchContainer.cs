using Spotify4Unity.Helpers;
using System.Collections.Generic;

/// <summary>
/// Helper class to inherit from to use the ExampleSearchController script
/// Check the wiki for more information
/// </summary>
/// <typeparam name="T"></typeparam>
public class SearchContainer<T> : LayoutGroupBase<T> where T : class
{
    /// <summary>
    /// Populates the list with the data
    /// </summary>
    /// <param name="list"></param>
    public void Populate(List<T> list)
    {
        UpdateUI(list);
    }
}