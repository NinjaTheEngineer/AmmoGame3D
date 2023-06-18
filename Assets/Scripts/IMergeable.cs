using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMergeable {
    public long Id{get; set;}
    public void MergeWith(IMergeable mergeable);
    public void OnMerge();
}
