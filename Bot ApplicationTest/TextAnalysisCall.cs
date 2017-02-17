using System;
using System.Collections.Generic;

[Serializable]
// Classes to store the input for the sentiment API call
public class TextInput
{
    public List<DocumentInput> documents { get; set; }
}

[Serializable]
public class DocumentInput
{
    public double id { get; set; }
    public string text { get; set; }
}

[Serializable]
// Classes to store the result from the sentiment analysis
public class BatchResult
{
    public List<DocumentResult> documents { get; set; }
}

[Serializable]
public class DocumentResult
{
    public double score { get; set; }
    public string id { get; set; }

    public List<string> keyPhrases { get; set; }

    public List<DetectedLanguages> detectedLanguages;
}

[Serializable]
public class DetectedLanguages
{
    public string name { get; set; }

    public string iso6391Name { get; set; }

    public double score { get; set; }
}