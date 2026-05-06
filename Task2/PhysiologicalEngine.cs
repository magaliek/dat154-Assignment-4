using SharedLogic;

namespace Task2;

public static class PhysiologicalEngine
{
    public static void ApplyEffect(CaseDto patient, string drug, double dose, string route)
    {
        switch (drug.ToLower())
        {
            case "labetalol":
                if (dose <= 20)
                {
                    patient.SystolicBP -= 25;
                    patient.HeartRate -= 10;
                }
                else
                {
                    patient.SystolicBP -= 35;
                    patient.HeartRate -= 12;
                }
                break;

            case "gtn":
            case "glyceryl trinitrate":
                patient.SystolicBP -= 15;
                break;

            case "oxygen":
                patient.OxygenSaturation += 4;
                break;

            case "glucose 20%":
            case "glucose":
                break;

            case "morphine":
                patient.RespiratoryRate -= 5;
                patient.OxygenSaturation -= 3;
                break;
        }

        patient.SystolicBP = Math.Max(60, patient.SystolicBP ?? 60);
        patient.DiastolicBP = Math.Max(40, patient.DiastolicBP ?? 40);
        patient.HeartRate = Math.Max(20, patient.HeartRate ?? 20);
        patient.OxygenSaturation = Math.Min(100, Math.Max(0, patient.OxygenSaturation ?? 0));
        patient.RespiratoryRate = Math.Max(0, patient.RespiratoryRate ?? 0);
    }
}
