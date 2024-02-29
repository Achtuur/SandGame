using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandGrid
{
    // singleton
    private static SandGrid m_Instance;
    public static SandGrid Instance
    {
        get
        {
            m_Instance ??= new SandGrid();
            return m_Instance;
        }
    }

    private static Grid m_FieldInstance;
    public static Grid Field
    {
        get
        {
            if (m_FieldInstance == null)
            {
                m_FieldInstance = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
            }
            return m_FieldInstance;
        }
    }
}
