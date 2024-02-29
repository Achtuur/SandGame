using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    const int COUNTER_SIZE = 10;

    [SerializeField]
    TextMeshProUGUI m_TextMesh;

    [SerializeField, Range(30f, 120f)]
    float m_TargetFrameRate = 60f;

    [SerializeField]
    Color m_GoodColor = Color.white;

    [SerializeField]
    Color m_BadColor = Color.red;
    
    float[] m_DeltaTimes = new float[COUNTER_SIZE];
    int m_DeltaTimeIndex = 0;


    // Update is called once per frame
    void Update()
    {
        m_DeltaTimes[m_DeltaTimeIndex] = Time.deltaTime;
        m_DeltaTimeIndex = (m_DeltaTimeIndex + 1) % COUNTER_SIZE;

        float avg_dt = AverageDeltaTime();
        float fps = 1f / avg_dt;

        if (m_TextMesh is not null)
        {
            m_TextMesh.text = $"FPS: {fps:0}";
            m_TextMesh.faceColor = GetTextColor(fps);
        }
    }

    float AverageDeltaTime()
    {
        return m_DeltaTimes.Sum() / COUNTER_SIZE;
    }

    Color GetTextColor(float fps)
    {
        if (fps >= m_TargetFrameRate)
            return m_GoodColor;

        return ColorLerp(m_BadColor, m_GoodColor, fps / m_TargetFrameRate);
    }

    Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(
            Mathf.Lerp(a.r, b.r, t),
            Mathf.Lerp(a.g, b.g, t),
            Mathf.Lerp(a.b, b.b, t),
            Mathf.Lerp(a.a, b.a, t)
        );
    }

}
