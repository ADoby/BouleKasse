using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class AppData
{
    [System.Serializable]
    public class Order
    {
        public int OrderNumber = 0;
        public Dictionary<string, int> Data;
        public bool Finished = false;

        public float Sum = 0f;

        [FullSerializer.fsIgnore]
        public GameObject SpawnedOrder;

        [FullSerializer.fsIgnore]
        public ProductButton SpawnedButton;

        public void Add(string name)
        {
            if (!Data.ContainsKey(name))
                Data.Add(name, 0);
            Data[name]++;
        }

        public void Remove(string name)
        {
            if (Data.ContainsKey(name))
                Data[name]--;
        }
    }

    public List<Order> Customers;
    public Dictionary<string, Order> Teammembers;
}

public class AppController : MonoBehaviour
{
    public static UnityAction ResetOrderFinished;

    public float sum = 0f;
    public Text SumText;
    public Text StornoText;
    public Button BezahltButton;

    public Toggle Storno;

    public static UnityAction OnFinished;

    public PrinterManager Printer;
    public string FileName = "Data.json";

    public AppData Data;

    [Header("Checklist")]
    public GameObject InfoHolder;

    public Transform CheckListHolder;
    public Dictionary<string, ProductButton> CheckButtons = new Dictionary<string, ProductButton>();

    [Header("Orders")]
    public GameObject OrdersPanel;

    public Transform SoldHolder;
    public List<OrderPanel> SpawnedOrders;

    [Header("Team")]
    public GameObject TeamPanel;

    public GameObject TeamSoldPanel;
    public Transform TeamMemberHolder;
    public Transform TeamMemberSoldHolder;
    public string SelectedTeamMember = "";
    public InputField TeamMemberName;
    public Text TeamSumLabel;
    public Text TeamSelectedName;
    public Button AufschreibenButton;
    public GameObject DeleteButton;
    public Dictionary<string, ProductButton> SpawnedTeamMemberSold = new Dictionary<string, ProductButton>();

    public AppData.Order CurrentCustomer
    {
        get
        {
            return Data.Customers[Data.Customers.Count - 1];
        }
    }

    public AppData.Order CurrentTeamMember
    {
        get
        {
            if (!Data.Teammembers.ContainsKey(SelectedTeamMember))
                return null;
            return Data.Teammembers[SelectedTeamMember];
        }
    }

    public string Path
    {
        get
        {
            if (Application.isEditor)
            {
                return Application.streamingAssetsPath;
            }
            else
            {
                return Application.persistentDataPath;
            }
        }
    }

    public void StornoChanged(bool value)
    {
        if (StornoText == null)
            return;
        if (value)
            StornoText.text = string.Format("STORNO\nAn");
        else
            StornoText.text = string.Format("STORNO\nAus");
    }

    private string GetFilePath()
    {
        return string.Format("{0}/{1}", Path, FileName);
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (CheckButtons == null)
            CheckButtons = new Dictionary<string, ProductButton>();

        BezahltButton.interactable = false;
        AufschreibenButton.interactable = false;
        DeleteButton.SetActive(false);

        Load();
        Save();

        ButtonInfo.ButtonClicked -= OnProductButtonClicked;
        ButtonInfo.ButtonClicked += OnProductButtonClicked;
    }

    private void Add(string name)
    {
        Data.Customers[Data.Customers.Count - 1].Add(name);
    }

    private void Remove(string name)
    {
        Data.Customers[Data.Customers.Count - 1].Remove(name);
    }

    private void CreateCustomer()
    {
        Data.Customers.Add(new AppData.Order() { OrderNumber = Data.Customers.Count, Data = new Dictionary<string, int>() });
    }

    private void ClearCustomer()
    {
        Data.Customers[Data.Customers.Count - 1].Data = new Dictionary<string, int>();

        Clear();
    }

    private void Load()
    {
        if (System.IO.File.Exists(GetFilePath()))
        {
            string text = System.IO.File.ReadAllText(GetFilePath());
            Data = FullserializerAPI.Deserialize<AppData>(text);
        }
        if (Data == null)
            Data = new AppData();

        if (Data != null)
        {
            if (Data.Customers == null)
                Data.Customers = new List<AppData.Order>();

            if (Data.Customers.Count == 0)
            {
                CreateCustomer();
            }
            else
            {
                if (Data.Customers[Data.Customers.Count - 1].Data.Count > 0)
                {
                    CreateCustomer();
                }
            }

            for (int i = 0; i < Data.Customers.Count; i++)
            {
                SpawnOrder(Data.Customers[i]);
            }

            if (Data.Teammembers == null)
                Data.Teammembers = new Dictionary<string, AppData.Order>();
            foreach (var item in Data.Teammembers)
            {
                SpawnTeamMember(item.Key);
            }
        }
    }

    private void SpawnOrder(AppData.Order order)
    {
        if (order == null)
            return;
        if (order.Data == null)
            return;
        if (order.Data.Count == 0)
            return;
        foreach (var item in order.Data)
        {
            AddSpawnCheckValue(item.Key, item.Value);
        }

        if (!order.Finished)
        {
            GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("OrderHolder"));
            order.SpawnedOrder = go;
            go.transform.SetParent(SoldHolder);
            go.transform.localScale = Vector3.one;
            var panel = go.GetComponent<OrderPanel>();
            panel.Order = order;
            panel.Label.text = string.Format("Bestellung: {0}", order.OrderNumber);

            ProductButton button;
            foreach (var item in order.Data)
            {
                go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("CheckProduct"));
                go.transform.SetParent(panel.parent);
                go.transform.localScale = Vector3.one;

                button = go.GetComponent<ProductButton>();
                button.Label.text = item.Key;
                button.Count.text = item.Value.ToString();
            }
        }
    }

    private void AddSpawnCheckValue(string name, int amount)
    {
        SetSpawnCheckButton(name, GetSpawnCheckValue(name) + amount);
    }

    private void SetSpawnCheckButton(string name, int count)
    {
        if (!CheckButtons.ContainsKey(name))
        {
            GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("CheckProduct"));
            go.transform.SetParent(CheckListHolder);
            go.transform.localScale = Vector3.one;

            ProductButton button = go.GetComponent<ProductButton>();
            button.Label.text = name;

            CheckButtons.Add(name, button);
        }

        CheckButtons[name].Count.text = count.ToString();

        if (count == 0)
        {
            Destroy(CheckButtons[name].gameObject);
            CheckButtons.Remove(name);
        }
    }

    private int GetSpawnCheckValue(string name)
    {
        if (CheckButtons.ContainsKey(name))
        {
            int value = 0;
            if (int.TryParse(CheckButtons[name].Count.text, out value))
                return value;
        }
        return 0;
    }

    private void Save()
    {
        string text = FullserializerAPI.Serialize<AppData>(Data);
        System.IO.File.WriteAllText(GetFilePath(), text);
    }

    public void OnProductButtonClicked(ButtonInfo info)
    {
        if (Storno.isOn)
        {
            if (info.Count > 0)
            {
                info.Count--;
                info.UpdateText();
                sum -= info.Price;
                Remove(info.Name);
            }
        }
        else
        {
            info.Count++;
            info.UpdateText();
            sum += info.Price;
            Add(info.Name);
        }
        UpdateText();

        BezahltButton.interactable = sum > 0;
        AufschreibenButton.interactable = sum > 0;
    }

    private void UpdateText()
    {
        SumText.text = GetPriceText(sum);
    }

    public static string GetPriceText(float price)
    {
        return string.Format("{0:0.00}€", price).Replace(".", ",");
    }

    public void Finish()
    {
        Storno.isOn = false;

        Clear();

        SpawnOrder(Data.Customers[Data.Customers.Count - 1]);

        CreateCustomer();
        Save();
    }

    public void Clear()
    {
        sum = 0;
        UpdateText();
        BezahltButton.interactable = false;
        AufschreibenButton.interactable = false;

        if (OnFinished != null)
            OnFinished.Invoke();
    }

    public GameObject PWPanel;
    public string Pass = "1234";

    public void ShowInfo()
    {
        PasswortPanel.Finished = DoShowInfo;
        PasswortPanel.Cancel = CancelShowInfo;
        PWPanel.SetActive(true);
    }

    public void CancelShowInfo()
    {
        PWPanel.SetActive(false);
    }

    public void DoShowInfo(string pw)
    {
        if (!pw.Equals(Pass))
            return;
        CancelShowInfo();
        if (InfoHolder != null)
            InfoHolder.SetActive(true);
    }

    public void HideInfo()
    {
        if (InfoHolder != null)
            InfoHolder.SetActive(false);
    }

    public void ShowOrders()
    {
        if (OrdersPanel != null)
            OrdersPanel.SetActive(true);
    }

    public void HideOrders()
    {
        if (OrdersPanel != null)
            OrdersPanel.SetActive(false);
        if (ResetOrderFinished != null)
            ResetOrderFinished.Invoke();
    }

    public void Reset()
    {
        PasswortPanel.Finished = DoReset;
        PasswortPanel.Cancel = CancelReset;
        PWPanel.SetActive(true);
    }

    public void DoReset(string pw)
    {
        if (!pw.Equals(Pass))
            return;
        PWPanel.SetActive(false);

        for (int i = 0; i < Data.Customers.Count; i++)
        {
            if (Data.Customers[i].SpawnedOrder != null)
                Destroy(Data.Customers[i].SpawnedOrder);
        }

        Data = new AppData();
        Save();

        foreach (var item in CheckButtons)
        {
            if (item.Value != null)
                Destroy(item.Value.gameObject);
        }
        CheckButtons.Clear();
        Load();
    }

    public void CancelReset()
    {
        PWPanel.SetActive(false);
    }

    public void DeleteOrders()
    {
        for (int i = 0; i < Data.Customers.Count; i++)
        {
            if (Data.Customers[i] == null)
                continue;
            if (Data.Customers[i].Finished)
            {
                if (Data.Customers[i].SpawnedOrder != null)
                    Destroy(Data.Customers[i].SpawnedOrder);
            }
        }
        Save();
    }

    public void AddTeamMember()
    {
        SpawnTeamMember(TeamMemberName.text);
        Save();
    }

    public void SelectTeamMember(ProductButton button)
    {
        SelectedTeamMember = button.Name;
        TeamSelectedName.text = SelectedTeamMember;
        SpawnTeamMemberSold(SelectedTeamMember);

        foreach (var item in Data.Teammembers)
        {
            if (item.Value != null && item.Value.SpawnedButton != null)
            {
                item.Value.SpawnedButton.SetSelected(item.Key == SelectedTeamMember);
            }
        }

        TeamSumLabel.text = GetPriceText(CurrentTeamMember.Sum);
        ShowTeamSold();

        DeleteButton.SetActive(CurrentTeamMember.Sum == 0);
    }

    public string TeamMemberPrefab = "Teammember";
    public string TeamMemberSoldPrefab = "TeamMemberSold";

    private void SpawnTeamMember(string name)
    {
        if (!Data.Teammembers.ContainsKey(name))
        {
            var order = new AppData.Order();
            order.Data = new Dictionary<string, int>();
            Data.Teammembers.Add(name, order);
        }

        if (Data.Teammembers[name].SpawnedButton != null)
            return;

        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(TeamMemberPrefab));
        go.transform.SetParent(TeamMemberHolder);
        go.transform.localScale = Vector3.one;

        var button = go.GetComponent<ProductButton>();
        button.Name = name;
        button.Label.text = name;
        button.Clicked -= SelectTeamMember;
        button.Clicked += SelectTeamMember;
        Data.Teammembers[name].SpawnedButton = button;
    }

    private void SpawnTeamMemberSold(string name)
    {
        DespawnTeamMemberSold();

        SpawnedTeamMemberSold.Clear();
        if (!Data.Teammembers.ContainsKey(name))
            return;
        foreach (var item in Data.Teammembers[name].Data)
        {
            SpawnTeamMemberSoldButton(item.Key, item.Value);
        }
    }

    private void DespawnTeamMemberSold()
    {
        if (SpawnedTeamMemberSold != null)
        {
            foreach (var item in SpawnedTeamMemberSold)
            {
                if (item.Value != null)
                    Destroy(item.Value.gameObject);
            }
        }
    }

    private void SpawnTeamMemberSoldButton(string key, int value)
    {
        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(TeamMemberSoldPrefab));
        go.transform.SetParent(TeamMemberSoldHolder);
        go.transform.localScale = Vector3.one;

        var button = go.GetComponent<ProductButton>();
        button.Label.text = key;
        button.Count.text = value.ToString();
        SpawnedTeamMemberSold.Add(key, button);
    }

    public void AddToTeamMember()
    {
        if (CurrentTeamMember == null)
            return;

        Data.Teammembers[SelectedTeamMember].Sum += sum;

        foreach (var item in CurrentCustomer.Data)
        {
            if (!CurrentTeamMember.Data.ContainsKey(item.Key))
                CurrentTeamMember.Data.Add(item.Key, 0);

            CurrentTeamMember.Data[item.Key] += item.Value;

            if (!SpawnedTeamMemberSold.ContainsKey(item.Key))
                SpawnTeamMemberSoldButton(item.Key, 0);

            SpawnedTeamMemberSold[item.Key].Count.text = CurrentTeamMember.Data[item.Key].ToString();
        }

        TeamSumLabel.text = GetPriceText(CurrentTeamMember.Sum);

        Clear();
        Save();
        DeleteButton.SetActive(CurrentTeamMember.Sum == 0);
    }

    public void FinishTeamMember()
    {
        if (CurrentTeamMember == null)
            return;

        DespawnTeamMemberSold();

        CurrentTeamMember.Data.Clear();
        CurrentTeamMember.Sum = 0;

        TeamSumLabel.text = GetPriceText(0);
        TeamSelectedName.text = "";

        Save();
        DeleteButton.SetActive(CurrentTeamMember.Sum == 0);
    }

    public void DeleteTeamMember()
    {
        HideTeamSold();
        if (CurrentTeamMember != null)
            Destroy(CurrentTeamMember.SpawnedButton.gameObject);

        Data.Teammembers.Remove(SelectedTeamMember);
    }

    public void ShowTeam()
    {
        TeamPanel.SetActive(true);
    }

    public void HideTeam()
    {
        TeamPanel.SetActive(false);
    }

    public void ShowTeamSold()
    {
        TeamSoldPanel.SetActive(true);
    }

    public void HideTeamSold()
    {
        TeamSoldPanel.SetActive(false);
    }
}