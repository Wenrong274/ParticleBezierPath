using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hyhy
{
    [CustomEditor(typeof(ParticleBezierPath))]
    public class ParticleBezierPathEditor : Editor
    {
        private ParticleBezierPath m_particleBezierPath;
        private Transform[] m_particleNodes;

        private void OnEnable()
        {
            m_particleBezierPath = (ParticleBezierPath)target;
            m_particleNodes = m_particleBezierPath.controlPoints;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private void OnSceneGUI()
        {
            if (m_particleNodes == null || m_particleNodes.Length == 0)
                return;

            Vector3 initPos = m_particleBezierPath.transform.position;
            Handles.Label(initPos, "PB Path : " + m_particleBezierPath.name);

            if (m_particleNodes[0])
            {
                Handles.DrawLine(m_particleBezierPath.transform.position, m_particleNodes[0].position);
                Handles.Label(m_particleNodes[0].position, "Node 1");

            }
            for (int i = 1; i < m_particleNodes.Length; i++)
            {
                if (m_particleNodes[i])
                {
                    Handles.DrawLine(m_particleNodes[i - 1].position, m_particleNodes[i].position);
                    Handles.Label(m_particleNodes[i].position, "Node " + (i + 1));
                }
            }
        }
    }
}
