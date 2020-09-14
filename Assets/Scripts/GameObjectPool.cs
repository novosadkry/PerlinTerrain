using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public GameObject pooledObject;
    public Transform parent;

    [Space]
    public bool fillOnAwake;
    
    [SerializeField]
    private bool canGrow;
    public bool CanGrow 
    { 
        get => canGrow; 
        set => canGrow = value; 
    }

    [SerializeField]
    private bool canShrink;
    public bool CanShrink
    {
        get => canShrink;
        set => canShrink = value;
    }

    [Space]
    [SerializeField]
    private int maxAmount;
    public int MaxAmount 
    { 
        get => maxAmount; 
        set => maxAmount = value;
    }

    public int Count => pool.Count;

    [SerializeField]
    private List<GameObject> pool = new List<GameObject>();

    public void Awake()
    {
        if (fillOnAwake)
            Fill();
    }

    public void Remove(GameObject o)
    {
        Destroy(o);
        pool.Remove(o);

        if (canShrink && MaxAmount > 0)
            MaxAmount--;
    }

    public void Remove(ICollection<GameObject> o)
    {
        foreach (var v in o)
            Remove(v);
    }

    public void RemoveUnused()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject o in pool.ToList())
        {
            if (!o.activeInHierarchy)
                toRemove.Add(o);
        }

        Remove(toRemove);
    }

    public void Trim(Func<GameObject, bool> predicate)
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject o in pool)
        {
            if (predicate(o))
                toRemove.Add(o);
        }

        Remove(toRemove);
    }

    public void Fill()
    {
        int canAdd = MaxAmount - Count;

        for (int i = 0; i < canAdd; i++)
        {
            GameObject o;

            if (parent != null)
                o = Instantiate(pooledObject, parent);
            else
                o = Instantiate(pooledObject);

            o.SetActive(false);
            pool.Add(o);
        }
    }

    private GameObject Add()
    {
        Remove(pool.Where((c) => c == null).ToList());

        if (Count >= MaxAmount)
        {
            if (canGrow)
                MaxAmount++;
            else
                return null;
        }

        GameObject o;

        if (parent != null)
            o = Instantiate(pooledObject, parent);
        else
            o = Instantiate(pooledObject);

        o.SetActive(false);
        pool.Add(o);

        return o;
    }

    public GameObject GetPooled()
    {
        foreach (GameObject o in pool)
        {
            if (o != null && !o.activeInHierarchy)
                return o;
        }

        return Add();
    }
}
