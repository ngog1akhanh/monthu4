using System;
using System.Drawing;
using System.Windows.Forms;
using TourGuideSmart.Models;
using TourGuideSmart.Services;

namespace TourGuideSmart
{
    public class FormDetail : Form
    {
    private Tour _tour;

    // small type to represent voice option in the UI
    private class VoiceOption
    {
        public string Name { get; set; } = string.Empty;
        public string? Id { get; set; }
        public override string ToString() => Name;
    }

    PictureBox pic = new PictureBox();
    Label lblName = new Label();
    Label lblPrice = new Label();
    TextBox txtDesc = new TextBox();

    Button btnMap = new Button();
    Button btnSpeak = new Button();
    ComboBox cmbVoice = new ComboBox();

    public FormDetail(Tour tour)
    {
        _tour = tour;

        this.Text = "Chi tiết quán";
        this.Width = 500;
        this.Height = 500;

        // Image
        pic.Width = 400;
        pic.Height = 200;
        pic.Top = 10;
        pic.Left = 50;
        pic.BackColor = Color.LightGray; // placeholder
        pic.SizeMode = PictureBoxSizeMode.StretchImage;
        // load image from tour if available
        try
        {
            if (!string.IsNullOrEmpty(tour.ImagePath) && System.IO.File.Exists(tour.ImagePath))
            {
                var img = Image.FromFile(tour.ImagePath);
                pic.Image = img;
            }
        }
        catch { }

        // Name
        lblName.Text = tour.Name;
        lblName.Top = 220;
        lblName.Left = 50;
        lblName.Font = new Font("Segoe UI", 14, FontStyle.Bold);

        // Price
        lblPrice.Text = "Giá: " + tour.Price + "đ";
        lblPrice.Top = 260;
        lblPrice.Left = 50;

        // Description
        txtDesc.Text = tour.Description;
        txtDesc.Top = 300;
        txtDesc.Left = 50;
        txtDesc.Width = 380;
        txtDesc.Height = 80;
        txtDesc.Multiline = true;

        // Map Button
        btnMap.Text = "Xem bản đồ";
        btnMap.Top = 400;
        btnMap.Left = 50;
        btnMap.Click += BtnMap_Click;

        // Speak Button
        btnSpeak.Text = "Nghe giới thiệu";
        btnSpeak.Top = 400;
        btnSpeak.Left = 200;
        btnSpeak.Click += BtnSpeak_Click;

        // Voice selector (for cloud/local voices)
        cmbVoice.Top = 400;
        cmbVoice.Left = 360;
        cmbVoice.Width = 260;
        cmbVoice.DropDownStyle = ComboBoxStyle.DropDownList;
        // populate with options: first is system/local fallback, others are ElevenLabs voice ids
        cmbVoice.Items.Add(new VoiceOption { Name = "System default (local TTS)", Id = null });
        cmbVoice.Items.Add(new VoiceOption { Name = "Chị Google (ElevenLabs, vi-VN)", Id = "21m00Tcm4TlvDq8ikWAM" });
        // select default
        cmbVoice.SelectedIndex = 0;

        // Back button to return to previous view using NavigationService
        var btnBack = new Button { Text = "Quay lại", Top = 400, Left = 320, Width = 100 };
        btnBack.Click += (s, e) =>
        {
            // if running inside FormMain navigation host, use NavigationService
            foreach (Form open in Application.OpenForms)
            {
                if (open is FormMain)
                {
                    TourGuideSmart.Services.NavigationService.GoBack();
                    return;
                }
            }
            this.Close();
        };

        this.Controls.Add(pic);
        this.Controls.Add(lblName);
        this.Controls.Add(lblPrice);
        this.Controls.Add(txtDesc);
        this.Controls.Add(btnMap);
        this.Controls.Add(btnSpeak);
        this.Controls.Add(cmbVoice);
        this.Controls.Add(btnBack);
        // adaptive layout
        TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
    }

    private void BtnMap_Click(object sender, EventArgs e)
    {
        var query = Uri.EscapeDataString(_tour.Name + " Vinh Khanh District 4");
        var url = "https://www.google.com/maps?q=" + query + "&output=embed";
        // prefer navigation service if available
        foreach (Form open in Application.OpenForms)
        {
            if (open is FormMain)
            {
                var fm = new FormMap(url);
                TourGuideSmart.Services.NavigationService.Navigate(fm);
                return;
            }
        }

        var dialog = new FormMap(url);
        dialog.ShowDialog(this);
    }

    private void BtnSpeak_Click(object sender, EventArgs e)
    {
        string? voiceId = null;
        if (cmbVoice.SelectedItem is VoiceOption vo)
            voiceId = vo.Id;

        new SpeechService().Speak(_tour.Name + ". " + _tour.Description, voiceId);
    }
    }
}
