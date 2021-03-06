﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenShelter.Models;
using OpenShelter.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OpenShelter.Views.Admin
{
    public partial class AdminDownloadPage : ContentPage
    {
        private readonly IAttendanceRepository attendanceRepository;

        public AdminDownloadPage()
        {
            InitializeComponent();
            this.attendanceRepository = DependencyService.Get<IAttendanceRepository>();
        }

        async void OnShareClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtYear.Text))
            {
                await DisplayAlert("Aviso", "Indique o ano", "Ok");

                return;
            }

            if (!int.TryParse(this.txtYear.Text, out var _))
            {
                await DisplayAlert("Aviso", "Indique o ano como número", "Ok");

                return;
            }

            var monthPicker = pickerMonth.SelectedItem;

            if (monthPicker == null)
            {
                await DisplayAlert("Aviso", "Selecione um mês", "Ok");

                return;
            }

            var attendances = this.attendanceRepository.GetAll(a =>
                a.EnterTime.Date.Month == Convert.ToInt32(this.pickerMonth.SelectedItem) &&
                a.EnterTime.Date.Year == Convert.ToInt32(this.txtYear.Text));

            if (attendances == null || attendances.Count == 0)
            {
                await DisplayAlert("Aviso", "Sem dados para esse mês", "Ok");

                return;
            }

            var fn = string.Format("Attendances-{0}-{1}-{2}.{3}", txtYear.Text, pickerMonth.SelectedItem.ToString(), DateTime.Now.ToString("yyyyMMddHHmmss"), "csv");
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var file = Path.Combine(FileSystem.CacheDirectory, fn);

            using (var streamWriter = new StreamWriter(file, true))
            {
                streamWriter.Write(GetCsv(attendances));
            }

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = Title,
                File = new ShareFile(file),
            });

            await Navigation.PopAsync();
        }

        private string GetCsv(List<Attendance> attendances)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Nome,Tarefa,Entrada,Saida");
            sb.AppendLine();

            foreach (var attendance in attendances)
            {
                sb.AppendFormat(
                    "{0},{1},{2},{3}",
                    attendance.Name,
                    attendance.TaskType,
                    attendance.EnterTime.ToString("dd-MM-yyyy HH:mm"),
                    attendance.ExitTime == default ? string.Empty : attendance.ExitTime.ToString("dd-MM-yyyy HH:mm"));

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
