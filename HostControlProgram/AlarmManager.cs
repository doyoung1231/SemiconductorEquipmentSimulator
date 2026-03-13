using System;

namespace HostControlProgram
{
    public class AlarmManager
    {
        public event Action<EquipmentData> OnAlarm;

        public void Check(EquipmentData data)
        {
            if (data.AlarmLevel > 0)
                OnAlarm?.Invoke(data);
        }
    }
}