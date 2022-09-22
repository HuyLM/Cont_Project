﻿using AtoLib.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using TMPro;
using AtoLib;
using AtoLib.Helper;

public class EditBillPanel : DOTweenFrame
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtCreateDate;
    [SerializeField] private TextMeshProUGUI txtDeliveryDate;
    [SerializeField] private ProductRowCollector productRowCollector;

    [SerializeField] private TextMeshProUGUI txtTotalPrice;
    [SerializeField] private TextMeshProUGUI ipPaid;
    [SerializeField] private TextMeshProUGUI txtDebt;
    [SerializeField] private ButtonBase btnSave;
    [SerializeField] private ButtonBase btnCancel;
    [SerializeField] private ButtonBase btnChooseDelivery;

    private NewBill curBill;
    private NewBill originBill;
    private bool isAddNew;
    private bool canDelete;

    private void Start()
    {
        btnSave.onClick.AddListener(OnSaveButtonClicked);
        btnCancel.onClick.AddListener(OnCancelButtonClicked);
        btnChooseDelivery.onClick.AddListener(OnChooseDeliveryButtonClicked);
    }
    protected override void OnShow(Action onCompleted = null, bool instant = false)
    {
        base.OnShow(onCompleted, instant);
        ShowUI();
    }

    protected override void OnResume(Action onCompleted = null, bool instant = false)
    {
        base.OnResume(onCompleted, instant);
        ShowUI();
    }

    protected override void OnHide(Action onCompleted = null, bool instant = false)
    {
        base.OnHide(onCompleted, instant);
        UIHUD.Instance.GetActiveFrame<ListBillPanel>().Refresh();
    }

    public void Refresh()
    {
        ShowUI();
    }

    private void ShowUI()
    {
        ShowCreateDate();
        ShowDeliveryDate();
        ShowProducts();
        ShowName();
        ShowTotalPrice();
        ShowPaidText();
        ShowDebtText();
    }

    public void SetOpenBill(NewBill bill, bool isAddNew)
    {
        this.isAddNew = isAddNew;
        this.originBill = bill;
        canDelete = !isAddNew;
        if (curBill == null)
        {
            curBill = new NewBill();
        }
        NewBill.Transmission(originBill, curBill);
    }

    #region Products

    private void OpenEditProductPopup(NewProduct product, bool isAddNewProduct)
    {
        Pause();
        PopupHUD.Instance.GetFrame<EditProductPopup>().SetOpenProduct(product, isAddNewProduct);
        PopupHUD.Instance.Show<EditProductPopup>();
    }

    private void ShowProducts()
    {
        List<NewProduct> products = curBill.Products;
        productRowCollector.AddOnSelect(OnSelectProductRow).SetCapacity(products.Count).SetItems(products).Show();
    }
    private void OnSelectProductRow(ProductRowViewDisplayer displayer)
    {
        OpenEditProductPopup(displayer.Model, false);
    }

    private void OnAddProductButtonClicked()
    {
        NewProduct newProduct = new NewProduct(curBill);
        //curBill.AddProduct(newProduct);
        OpenEditProductPopup(newProduct, true);
    }

    #endregion

    #region Date
    private void ShowCreateDate()
    {
        txtCreateDate.text = curBill.CreateDate.ToString("dd/MM/yyyy HH:mm");
    }

    private void ShowDeliveryDate()
    {
        txtDeliveryDate.text = curBill.DeliveryDate.ToString("dd/MM/yyyy HH:mm");
    }

    #endregion

    #region Combox Name
    private void ShowName()
    {
        txtName.text = curBill.Customer.CustomerName;
    }

    #endregion

    #region Buttons
    private void OnSaveButtonClicked()
    {
        if (curBill == null)
        {
            curBill = new NewBill();
        }
        NewBill.Transmission(curBill, originBill);
        originBill.SetDirty();
        if (isAddNew)
        {
            curBill.Customer.AddBill(originBill);
        }
        Hide();
    }

    private void OnCancelButtonClicked()
    {
        Hide();
    }

    private void OnDeleteButtonClicked()
    {
        ConfirmPopup confirmPopup = PopupHUD.Instance.Show<ConfirmPopup>();
        confirmPopup.SetTile(null, false);
        confirmPopup.SetMessage("Bạn có muốn xoá không?");
        confirmPopup.SetConfirmText("Xoá");
        confirmPopup.SetCancelText("Thoát");
        confirmPopup.SetOnConfirm(() =>
        {
            originBill.Customer.RemoveBill(originBill);
            Hide();
        });
    }

    private void OnChooseDeliveryButtonClicked()
    {
       ChooseDateTimePopup chooseDateTimePopup =  PopupHUD.Instance.Show<ChooseDateTimePopup>().SetDateTime(curBill.DeliveryDate);
        chooseDateTimePopup.SetOnConfirm((dateTimeString, dateTime) => {
            curBill.DeliveryDate = dateTime;
            ShowUI();
        });
    }
    #endregion

    #region Total, Debt, Paid
    private void ShowTotalPrice()
    {
        txtTotalPrice.text = StringHelper.GetCommaCurrencyFormat(curBill.TotalPrice);
    }

    private void ShowPaidText()
    {
        ipPaid.text = StringHelper.GetCommaCurrencyFormat(curBill.Paid);
    }

    private void ShowDebtText()
    {
        txtDebt.text = StringHelper.GetCommaCurrencyFormat(curBill.Debt);
    }

    private void OnPaidEndEdit(string text)
    {
        string valText = text.Replace(",", string.Empty);
        if (string.IsNullOrEmpty(valText))
        {
            curBill.SetPaid(0);
        }
        else
        {
            curBill.SetPaid(int.Parse(valText));
        }
        ShowUI();
    }

    private void OnPaidOnValueChanged(string text)
    {
        string valText = text.Replace(",", string.Empty);
        if (string.IsNullOrEmpty(valText))
        {
            curBill.SetPaid(0);
        }
        else
        {
            int valInt = int.Parse(valText);
            valInt = Mathf.Clamp(valInt, 0, curBill.TotalPrice);
            curBill.SetPaid(valInt);
        }
        ShowUI();
    }
    #endregion
}
