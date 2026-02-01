using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Float" , menuName = "Serialized Value/Float")]
public class SerializedFloat : ScriptableObject
{
    [SerializeField] private float value;
    
    public event Action<float> OnValueChanged;

    public float Value
    {
        get => value;
        set
        {
            if (Mathf.Approximately(this.value, value))
                return;

            this.value = value;
            OnValueChanged?.Invoke(this.value);
        }
    }
}
