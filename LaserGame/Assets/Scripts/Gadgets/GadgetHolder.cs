using UnityEngine;

// Fica no Player. É o único lugar que sabe "qual gadget está equipado agora"
// e o único lugar que lê input pra decidir SE deve usar o gadget.
public class GadgetHolder : MonoBehaviour
{
    [Header("Equip Point")]
    [Tooltip("Onde o gadget instanciado vai ficar (mão, socket, etc.)")]
    [SerializeField] private Transform gadgetSocket;

    [Header("Gadgets disponíveis")]
    [SerializeField] private GadgetData[] availableGadgets;

    private BaseGadget currentGadget;
    private GameObject currentGadgetInstance;
    private int currentIndex = -1;

    // --- "Sistema pra verificar o currentGadget" ---
    // Outros scripts (UI de munição, HUD do gadget equipado, etc.) podem
    // ler essas propriedades sem nunca precisar saber COMO o gadget foi
    // instanciado ou equipado.
    public GadgetData CurrentGadgetData => currentGadget != null ? currentGadget.GadgetData : null;
    public bool HasGadgetEquipped => currentGadget != null;
    public int CurrentIndex => currentIndex;

    private void Start()
    {
        // Equipa o primeiro gadget da lista automaticamente, se existir.
        if (availableGadgets.Length > 0)
            EquipGadget(0);
    }

    private void Update()
    {
        HandleSwitchInput();
        HandleUseInput();
    }

    // Passo 1: trocar de gadget (exemplo simples com teclas 1, 2, 3...).
    // Ajuste pro seu esquema de input real (scroll, D-pad, etc.).
    private void HandleSwitchInput()
    {
        for (int i = 0; i < availableGadgets.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipGadget(i);
                break;
            }
        }
    }

    // Passo 2: usar o gadget equipado. Note que aqui só existe UMA
    // chamada de Input pro "atirar/usar" — antes isso estava duplicado
    // dentro do próprio RangedWeapon, o que causava o bypass do cooldown.
    private void HandleUseInput()
    {
        if (currentGadget == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            currentGadget.UseGadget(); // cooldown é checado DENTRO desse método
        }
    }

    // Passo 3: trocar/equipar de fato.
    public void EquipGadget(int index)
    {
        if (index < 0 || index >= availableGadgets.Length) return;
        if (index == currentIndex) return; // já equipado, evita reinstanciar à toa

        currentGadgetInstance.gameObject.SetActive(false);
        GadgetData data = availableGadgets[index];
        if (data == null || data.gadgetPrefab == null)
        {
            Debug.LogWarning($"GadgetData no índice {index} está incompleto (falta prefab).");
            return;
        }

        // Remove o gadget atual antes de instanciar o novo.
        if (currentGadgetInstance != null)
            Destroy(currentGadgetInstance);

        // O prefab já vem com o componente BaseGadget (RangedWeapon, etc.)
        // configurado no Inspector com o GadgetData correspondente — o
        // GadgetHolder não precisa (e não deve) injetar dados manualmente.
        currentGadgetInstance = Instantiate(data.gadgetPrefab, gadgetSocket.position, gadgetSocket.rotation, gadgetSocket);
        currentGadget = currentGadgetInstance.GetComponent<BaseGadget>();

        if (currentGadget == null)
        {
            Debug.LogError($"O prefab de '{data.gadgetName}' não tem nenhum componente que herde de BaseGadget.");
            Destroy(currentGadgetInstance);
            currentGadgetInstance = null;
            currentIndex = -1;
            return;
        }

        currentIndex = index;
    }

    public void UnequipCurrentGadget()
    {
        if (currentGadgetInstance != null)
            Destroy(currentGadgetInstance);

        currentGadgetInstance.gameObject.SetActive(false);
        currentGadget = null;
        currentGadgetInstance = null;
        currentIndex = -1;

    }
}