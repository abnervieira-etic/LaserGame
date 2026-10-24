using UnityEngine;

public abstract class BaseGadget : MonoBehaviour
{
    [SerializeField] protected GadgetData gadgetData;
    protected float lastUsedTime;

    // Exposto como leitura pra quem precisar saber os dados do gadget
    // (o GadgetHolder, por exemplo) sem poder alterá-los.
    public GadgetData GadgetData => gadgetData;

    // Chamado de FORA (pelo GadgetHolder), nunca pelo próprio gadget.
    // É aqui que o cooldown é verificado — se o gadget que efetivamente
    // dispara já vier daqui, o cooldown é respeitado sempre, não importa
    // quem chamou.
    public virtual void UseGadget()
    {
        if (Time.time - lastUsedTime >= gadgetData.cooldownTime)
        {
            lastUsedTime = Time.time;
            ActivateGadget();
        }
    }

    // Cada tipo de gadget implementa o próprio efeito aqui.
    protected abstract void ActivateGadget();
}