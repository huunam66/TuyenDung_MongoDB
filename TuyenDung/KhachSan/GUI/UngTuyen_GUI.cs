﻿using KhachSan.DAO;
using KhachSan.DTO;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using Amazon.Runtime.Documents;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Xml.Linq;

    namespace KhachSan.GUI
    {
   

        public partial class UngTuyen_GUI : Form
        {
            private MongoClient client;
            private IMongoDatabase database;
            private IMongoCollection<DTO.ViTriTuyenDung> collection;


            public UngTuyen_GUI()
            {
                InitializeComponent();
                // Chuỗi kết nối tới MongoDB
                string connectionString = "mongodb://localhost:27017";
                // Tên cơ sở dữ liệu
                string databaseName = "TuyenDung";
                // Tên bảng (collection)
                string collectionName = "ViTriTuyenDung";

                // Tạo kết nối MongoDB
                client = new MongoClient(connectionString);
                // Chọn cơ sở dữ liệu
                database = client.GetDatabase(databaseName);
            // Chọn bảng (collection)

             collection = database.GetCollection<DTO.ViTriTuyenDung>(collectionName);

                LoadDataToDataGridView();
                LoadComboBoxData();
            dt_ungvien.CellClick += dt_ungvien_CellClick;

            anThuocTinh();
            btn_duyet.Enabled = false;

        }

        private void anThuocTinh()
        {
            txt_CongViec.Visible = false;
            txt_ThoiGianLV.Visible = false;
            lbl_congViec.Visible = false;
            lbl_thoiGian.Visible = false;
        }
         
        public void LoadDataToDataGridView()
        {
            dt_ungvien.Columns.Clear();
            dt_ungvien.Rows.Clear();

            // Tạo các cột tương ứng với thuộc tính của ThiSinhUngTuyen
            foreach (var property in typeof(DTO.ThiSinhUngTuyen).GetProperties())
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = property.Name;
                column.HeaderText = property.Name;
                dt_ungvien.Columns.Add(column);
            }

            // Thêm cột ID vào DataGridView
            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = "ID";
            idColumn.HeaderText = "ID";
            dt_ungvien.Columns.Add(idColumn);

            // Thêm cột TenViTri vào DataGridView
            DataGridViewTextBoxColumn tenViTriColumn = new DataGridViewTextBoxColumn();
            tenViTriColumn.Name = "TenViTri";
            tenViTriColumn.HeaderText = "TenViTri";
            dt_ungvien.Columns.Add(tenViTriColumn);

            // Truy vấn tất cả các bản ghi từ bảng (collection) ViTriTuyenDung
            var documents = collection.Find(_ => true).ToList();

            // Thêm dữ liệu từng document vào DataGridView
            foreach (var document in documents)
            {
                // Lấy danh sách ThiSinhUngTuyen từ document
                List<DTO.ThiSinhUngTuyen> thiSinhUngTuyenList = document.ThiSinhUngTuyen;

                // Lặp qua từng ThiSinhUngTuyen trong danh sách
                foreach (var thiSinhUngTuyen in thiSinhUngTuyenList)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dt_ungvien);

                    foreach (DataGridViewColumn column in dt_ungvien.Columns)
                    {
                        string propertyName = column.Name;
                        var property = typeof(DTO.ThiSinhUngTuyen).GetProperty(propertyName);

                        if (propertyName == "KinhNghiem")
                        {
                            var kinhNghiemList = (List<DTO.KinhNghiem>)property.GetValue(thiSinhUngTuyen);
                            string kinhNghiemString = string.Join(Environment.NewLine, kinhNghiemList.Select(kn => $"{kn.CongViec}: {kn.KhoangThoiGian}. "));
                            row.Cells[column.Index].Value = kinhNghiemString;
                        }
                        else if (propertyName == "KyNang")
                        {
                            var kyNangList = (List<string>)property.GetValue(thiSinhUngTuyen);
                            string kyNangString = string.Join(", ", kyNangList);
                            row.Cells[column.Index].Value = kyNangString;
                        }
                        else if (propertyName == "TenViTri") // Added this condition to handle TenViTri
                        {
                            row.Cells[column.Index].Value = document.TenViTri; // Retrieve the TenViTri from the document
                        }
                        else if (propertyName == "ID") // Added this condition to handle ID
                        {
                            row.Cells[column.Index].Value = document._id; // Retrieve the ID from the document
                        }
                        else
                        {
                            var value = property.GetValue(thiSinhUngTuyen);
                            row.Cells[column.Index].Value = value;
                        }
                    }
                    dt_ungvien.Rows.Add(row);
                }
            }
        }
        private void LoadComboBoxData()
        {
            var documents = collection.Find(_ => true).ToList();
            List<string> tenViTris = new List<string>();

            foreach (var document in documents)
            {
                tenViTris.Add(document.TenViTri.ToString() + " - " + document._id.ToString());
            }

            cbbox_Id.DataSource = tenViTris;
        }

         
        private void dt_ungvien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dt_ungvien.Rows.Count - 1)
            {

                btn_duyet.Enabled = true;
                DataGridViewRow selectedRow = dt_ungvien.Rows[e.RowIndex];
               
                string id = selectedRow.Cells["ID"].ToString();
                string maThiSinh = selectedRow.Cells["MaThiSinh"].Value.ToString();
                string hoTen = selectedRow.Cells["HoTen"].Value.ToString();
                string phai = selectedRow.Cells["Phai"].Value.ToString();
                DateTime ngaySinh = (DateTime)selectedRow.Cells["NgaySinh"].Value;
                string email = selectedRow.Cells["Email"].Value.ToString();
                DateTime ngayUngTuyen = (DateTime)selectedRow.Cells["NgayUngTuyen"].Value;
                string trangThai = selectedRow.Cells["TrangThai"].Value.ToString();
                string kinhNghiem = selectedRow.Cells["KinhNghiem"].Value.ToString();
                string kyNang = selectedRow.Cells["KyNang"].Value.ToString();
                string linkCV = selectedRow.Cells["LinkCV"].Value.ToString();
                string tenvitri = selectedRow.Cells["TenViTri"].Value.ToString();

                txt_id.Text = maThiSinh;
                txt_name.Text = hoTen;
                txt_Phai.Text = phai;
                dateTimePicker_birthDay.Value = ngaySinh;
                txt_email.Text = email;
                dateTimePicker_Ngayungtuyen.Value = ngayUngTuyen;
                txt_TrangThai.Text = trangThai;
                txt_kinhNghiem.Text = kinhNghiem;
                txt_Kynang.Text = kyNang;
                txt_CV.Text = linkCV;

                // Chỉnh lại combobox
                string selectedValue = tenvitri + " - " + selectedRow.Cells["ID"].Value.ToString();
                if (cbbox_Id.Items.Contains(selectedValue))
                {
                    cbbox_Id.SelectedItem = selectedValue;
                }

                if (e.ColumnIndex == 9)
                {
                    Process.Start(linkCV);
                }
            }
        }

      
        private void btn_delete_Click(object sender, EventArgs e)
        {
            string maThiSinh = txt_id.Text;
            string idViTriTuyenDung = cbbox_Id.Text.Split('-')[1].Trim();

            var filter = Builders<DTO.ViTriTuyenDung>.Filter.And(
                Builders<DTO.ViTriTuyenDung>.Filter.Eq("_id", ObjectId.Parse(idViTriTuyenDung)),
                Builders<DTO.ViTriTuyenDung>.Filter.ElemMatch(
                    "ThiSinhUngTuyen",
                    Builders<DTO.ThiSinhUngTuyen>.Filter.Eq("MaThiSinh", maThiSinh)
                )
            );

            var update = Builders<DTO.ViTriTuyenDung>.Update.PullFilter(
                "ThiSinhUngTuyen",
                Builders<DTO.ThiSinhUngTuyen>.Filter.Eq("MaThiSinh", maThiSinh)
            );

            collection.UpdateOne(filter, update);

            LoadDataToDataGridView();


        }


         
        private void btn_add_Click(object sender, EventArgs e)
        {
             
            string maThiSinh = txt_id.Text;
            string hoTen = txt_name.Text;
            string phai = txt_Phai.Text;
            DateTime ngaySinh = dateTimePicker_birthDay.Value;
            string email = txt_email.Text;
            DateTime ngayUngTuyen = dateTimePicker_Ngayungtuyen.Value;
            string trangThai = "Ứng tuyển";

            string[] congViecArray = txt_CongViec.Text.Split(',');
            string[] thoiGianArray = txt_ThoiGianLV.Text.Split(',');
             
            List<DTO.KinhNghiem> kinhNghiemList = new List<DTO.KinhNghiem>();
            for (int i = 0; i < congViecArray.Length; i++)
            {
                DTO.KinhNghiem kinhNghiem = new DTO.KinhNghiem()
                {
                    CongViec = congViecArray[i].Trim(),
                    KhoangThoiGian = thoiGianArray[i].Trim()
                };
                kinhNghiemList.Add(kinhNghiem);
            }

            string kyNang = txt_Kynang.Text;
            string linkCV = txt_CV.Text;
            string tenViTri = cbbox_Id.SelectedItem.ToString();

            String[] skills = kyNang.Split(',');
            for(int i = 0; i < skills.Length; i++) { skills[i] = skills[i].Trim(); }
             
            DTO.ThiSinhUngTuyen thiSinhUngTuyen = new DTO.ThiSinhUngTuyen()
            {
                MaThiSinh = maThiSinh,
                HoTen = hoTen,
                Phai = phai,
                NgaySinh = ngaySinh,
                Email = email,
                NgayUngTuyen = ngayUngTuyen,
                TrangThai = trangThai,
                KinhNghiem = kinhNghiemList,
                KyNang = skills.ToList(),
                LinkCV = linkCV
            };
             
            var filter = Builders<DTO.ViTriTuyenDung>.Filter.Eq("_id", ObjectId.Parse(tenViTri.Split('-')[1].Trim()));
              
            var update = Builders<DTO.ViTriTuyenDung>.Update.Push("ThiSinhUngTuyen", thiSinhUngTuyen);
             
            collection.UpdateOne(filter, update);
             
            LoadDataToDataGridView();
            anThuocTinh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dt_ungvien.Columns.Clear();

        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            txt_CongViec.Visible = true;
            txt_ThoiGianLV.Visible = true;
            lbl_congViec.Visible = true;
            lbl_thoiGian.Visible = true;
        }


         

        private void btn_edit_Click(object sender, EventArgs e)
        {
            // Lấy mã thí sinh cần sửa
            string maThiSinh = txt_id.Text;
            string tenViTri = cbbox_Id.SelectedItem.ToString();

            string hoTen = txt_name.Text;
            string phai = txt_Phai.Text;
            DateTime ngaySinh = dateTimePicker_birthDay.Value;
            string email = txt_email.Text;
            DateTime ngayUngTuyen = dateTimePicker_Ngayungtuyen.Value;

            string trangThai = "Ứng tuyển";

            string[] congViecArray = txt_CongViec.Text.Split(',');
            string[] thoiGianArray = txt_ThoiGianLV.Text.Split(',');

            List<DTO.KinhNghiem> kinhNghiemList = new List<DTO.KinhNghiem>();
            for (int i = 0; i < congViecArray.Length; i++)
            {
                DTO.KinhNghiem kinhNghiem = new DTO.KinhNghiem()
                {
                    CongViec = congViecArray[i].Trim(),
                    KhoangThoiGian = thoiGianArray[i].Trim()
                };
                kinhNghiemList.Add(kinhNghiem);
            }

            string kyNang = txt_Kynang.Text;
            string linkCV = txt_CV.Text;

            String[] skills = kyNang.Split(',');
            for (int i = 0; i < skills.Length; i++) { skills[i] = skills[i].Trim(); }

            var filter = Builders<DTO.ViTriTuyenDung>.Filter.And(
                Builders<DTO.ViTriTuyenDung>.Filter.Eq("ThiSinhUngTuyen.MaThiSinh", maThiSinh),
                Builders<DTO.ViTriTuyenDung>.Filter.Eq("_id", ObjectId.Parse(tenViTri.Split('-')[1].Trim()))
            );

            var updateFilter = Builders<DTO.ViTriTuyenDung>.Filter.And(
                Builders<DTO.ViTriTuyenDung>.Filter.ElemMatch(x => x.ThiSinhUngTuyen, t => t.MaThiSinh == maThiSinh),
                Builders<DTO.ViTriTuyenDung>.Filter.Eq("_id", ObjectId.Parse(tenViTri.Split('-')[1].Trim()))
            );

            var update = Builders<DTO.ViTriTuyenDung>.Update
                .Set("ThiSinhUngTuyen.$.HoTen", hoTen)
                .Set("ThiSinhUngTuyen.$.Phai", phai)
                .Set("ThiSinhUngTuyen.$.NgaySinh", ngaySinh)
                .Set("ThiSinhUngTuyen.$.Email", email)
                .Set("ThiSinhUngTuyen.$.NgayUngTuyen", ngayUngTuyen)
                .Set("ThiSinhUngTuyen.$.KinhNghiem", kinhNghiemList)
                .Set("ThiSinhUngTuyen.$.KyNang", skills.ToList())
                .Set("ThiSinhUngTuyen.$.LinkCV", linkCV);

            collection.UpdateOne(updateFilter, update);

            LoadDataToDataGridView();
            anThuocTinh();
        }
   

        private void button2_Click(object sender, EventArgs e)
        {
            if (!dt_ungvien.CurrentCell.Selected
                || dt_ungvien.CurrentCell.RowIndex
                == dt_ungvien.Rows.Count - 1) return;

            
            String idVT = cbbox_Id.Text.Split('-')[1].Trim();
            String idTS = txt_id.Text.ToString();
            DTO.ThiSinhTrungTuyen ts = (DTO.ThiSinhTrungTuyen)db.ThiSinhTrungTuyen.findOne(idTS, ObjectId.Parse(idVT));

            if(ts == null) {
                db.ThiSinhUngTuyen.DuyetUngTuyen(idTS, ObjectId.Parse(idVT));
                MessageBox.Show("Thí sinh đã được duyệt thành công !", "Thông báo");
                LoadDataToDataGridView();
                btn_duyet.Enabled = false;
                return;
            }

            MessageBox.Show("Đã trúng tuyển trước đó !", "Thông báo");
            btn_duyet.Enabled = false;
        }

        private void btntimkiem_Click(object sender, EventArgs e)
        {
            string hoTenTimKiem = btn_Timkiem.Text;

            // Tạo một bộ lọc (filter) để tìm kiếm theo họ tên
            var filter = Builders<DTO.ViTriTuyenDung>.Filter.ElemMatch("ThiSinhUngTuyen", Builders<DTO.ThiSinhUngTuyen>.Filter.Regex("HoTen", new BsonRegularExpression(hoTenTimKiem, "i")));

            // Truy vấn các bản ghi thỏa mãn bộ lọc từ collection
            var documents = collection.Find(filter).ToList();

            // Xóa dữ liệu cũ trong DataGridView
            dt_ungvien.Rows.Clear();

            // Thêm dữ liệu từ các bản ghi tìm được vào DataGridView
            foreach (var document in documents)
            {
                List<DTO.ThiSinhUngTuyen> thiSinhUngTuyenList = document.ThiSinhUngTuyen;

                foreach (var thiSinhUngTuyen in thiSinhUngTuyenList)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dt_ungvien);

                    // Thiết lập giá trị cho mỗi cột tương ứng
                    row.Cells[0].Value = thiSinhUngTuyen.MaThiSinh;
                    row.Cells[1].Value = thiSinhUngTuyen.HoTen;
                    row.Cells[2].Value = thiSinhUngTuyen.Phai;
                    row.Cells[3].Value = thiSinhUngTuyen.NgaySinh;
                    row.Cells[4].Value = thiSinhUngTuyen.Email;
                    row.Cells[5].Value = thiSinhUngTuyen.NgayUngTuyen;
                    row.Cells[6].Value = thiSinhUngTuyen.TrangThai;




                    //Thêm các cột khác tương tự

                    dt_ungvien.Rows.Add(row);
                }
            }
        }

        private void cbbox_Id_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDataToDataGridView();
        }
    }
}
