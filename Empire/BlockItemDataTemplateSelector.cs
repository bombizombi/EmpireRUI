using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Empire.ViewModels;


namespace Empire
{
    public class BlockItemDataTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;

            if (element != null && item != null)  // && item is WhatType
            {
                CellViewModel? cvm = item as CellViewModel;
                if( cvm != null)
                {

                    if (cvm.ImageName == "")
                    {
                        var resc = element.FindResource("blockTemplate_City");
                        return resc as DataTemplate;

                    }

                    var res = element.FindResource("blockTemplate_Image");
                    DataTemplate? dt = res as DataTemplate;
                    return dt;
                }


            }
/*
            if (element != null && item != null && item is AuctionItem)
            {
                AuctionItem auctionItem = item as AuctionItem;
                Window window = Application.Current.MainWindow;

                switch (auctionItem.SpecialFeatures)
                {
                    case SpecialFeatures.None:
                        return
                            element.FindResource("AuctionItem_None")
                            as DataTemplate;
                    case SpecialFeatures.Color:
                        return
                            element.FindResource("AuctionItem_Color")
                            as DataTemplate;
                }
            }
*/
            return null;


        }
    }
}
