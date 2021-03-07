using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32; // Mở cửa sổ file hình

namespace WpfApp2_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelHinh dc = new ModelHinh();
        private string path = "";

        // chứa tên file temp khi user mở file hình
        private string tenfileHinhTemp = "";

        // cột móc nhận biết ảnh user: đã thêm mới, đã để nguyên ko có ảnh, đã xóa ảnh
        private bool flagChange = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // quay về 3 lần để vào folder HinhAnh từ chỗ: D: > Nam4_HKIIWin > WpfApp2_2 > WpfApp2_2 > bin > Debug >
            // MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
            // lấy thư mục cha của thư mục hiện tại: Directory.GetParent
            DirectoryInfo di = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            di = di.Parent;
            di = di.Parent;
            path = di.FullName + @"\HinhAnh\";
            dgHocvien.ItemsSource = dc.hocviens.ToList();
        }

        private void dgHocvien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // lá cờ bẫy cho change img khi upd
            flagChange = false;

            if (dgHocvien.SelectedItem == null) return;
            hocvien hv = dgHocvien.SelectedItem as hocvien;
            txtMshv.Text = hv.mshv;
            txtTenhv.Text = hv.tenhv;

            // tránh việc đã mở file trc đó: ngắt connect tới file để tạo connect mới
            BitmapImage bmTemp = imgHinh.Source as BitmapImage;
            if (bmTemp != null)
            {
                bmTemp.StreamSource.Close();
                imgHinh.Source = null;
            }

            if (hv.hinh != "")
            {
                // liên kết bm hình gián tiếp thông qua file stream
                string tenfile = path + hv.hinh;
                BitmapImage bm = new BitmapImage();

                FileStream f = new FileStream(tenfile, FileMode.Open); // liên kết hình trên đĩa trc
                bm.BeginInit();
                bm.StreamSource = f; // liên kết bm vs file stream
                bm.EndInit();
            
                imgHinh.Source = bm;
            } else
            {
                imgHinh.Source = null;
            }
        }

        private void btnThuchien_Click(object sender, RoutedEventArgs e)
        {
            if (rdoThem.IsChecked == true)
            {
                // Check ràng buộc khóa chính trc khi thêm
                hocvien hv_old = dc.hocviens.Find(txtMshv.Text);
                if (hv_old != null)
                {
                    MessageBox.Show("Tồn tại HV này!");
                    return;
                }
                if (txtMshv.Text == "")
                {
                    MessageBox.Show("Bạn chưa nhập MSHV!");
                    return;
                }
                if(txtTenhv.Text == "")
                {
                    MessageBox.Show("Bạn chưa nhập Tên HV!");
                    return;
                }

                hocvien hv = new hocvien();
                hv.mshv = txtMshv.Text;
                hv.tenhv = txtTenhv.Text;

                if (imgHinh.Source == null) hv.hinh = "";
                else
                {
                    BitmapImage bmTemp = imgHinh.Source as BitmapImage;
                    FileStream f = bmTemp.StreamSource as FileStream;
                    
                    // lấy phẩn mở rộng
                    FileInfo fi = new FileInfo(f.Name);
                    // hình: mshv.png
                    hv.hinh = hv.mshv + fi.Extension;
                    
                    f.Close();

                    // sao chép hình từ folder ngoài vào /HinhAnh
                    File.Copy(tenfileHinhTemp, path + hv.hinh);
                    imgHinh.Source = null;
                }

                dc.hocviens.Add(hv);
                dc.SaveChanges();
                dgHocvien.ItemsSource = dc.hocviens.ToList();
            }
            else if (rdoXoa.IsChecked == true)
            {
                // xóa info hv trong CSDL -> xóa img trong /HinhAnh (Cắt -> Xóa)
                hocvien hv = dc.hocviens.Find(txtMshv.Text);

                // tránh việc đã mở file trc đó: ngắt connect tới file để tạo connect mới
                BitmapImage bmTemp = imgHinh.Source as BitmapImage;
                if (bmTemp != null)
                {
                    bmTemp.StreamSource.Close();
                    imgHinh.Source = null;
                }
                if(hv.hinh!="")
                    File.Delete(path + hv.hinh);
                dc.hocviens.Remove(hv);
                dc.SaveChanges();
                dgHocvien.ItemsSource = dc.hocviens.ToList();

                txtMshv.Text = "";
                txtTenhv.Text = "";
            }
            else if(rdoSua.IsChecked == true)
            {
                // cắt link img cũ -> chép đè lên
                hocvien hv = dc.hocviens.Find(txtMshv.Text);
                hv.tenhv = txtTenhv.Text;

                if (flagChange)
                {
                    //MessageBox.Show("changed " + tenfileHinhTemp);
                    if (hv.hinh != "")
                        File.Delete(path + hv.hinh);

                    if (tenfileHinhTemp == "")
                    {
                        hv.hinh = "";
                    }
                    else
                    {
                        // tránh việc đã mở file trc đó: ngắt connect tới file để tạo connect mới
                        BitmapImage bmTemp = imgHinh.Source as BitmapImage;
                        if (bmTemp != null)
                        {
                            bmTemp.StreamSource.Close();
                        }

                        bmTemp = imgHinh.Source as BitmapImage;
                        FileStream f = bmTemp.StreamSource as FileStream;

                        // lấy phẩn mở rộng
                        FileInfo fi = new FileInfo(f.Name);
                        // hình: mshv.png
                        hv.hinh = hv.mshv + fi.Extension;

                        f.Close();

                        // sao chép hình từ folder ngoài vào /HinhAnh
                        File.Copy(tenfileHinhTemp, path + hv.hinh);
                    }
                }
                dc.SaveChanges();
                dgHocvien.ItemsSource = dc.hocviens.ToList();
            }
            // lá cờ bẫy cho change img khi upd
            flagChange = false;
        }

        private void btnChonhinh_Click(object sender, RoutedEventArgs e)
        {
            // lá cờ bẫy cho change img khi upd
            flagChange = true;

            // mở cửa sổ file hình
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                // tránh việc đã mở file trc đó: ngắt connect tới file để tạo connect mới
                BitmapImage bmTemp = imgHinh.Source as BitmapImage;
                if (bmTemp != null)
                {
                    bmTemp.StreamSource.Close();
                    imgHinh.Source = null;
                }

                tenfileHinhTemp = dlg.FileName; // lấy tên file hình user chọn

                // Hiện tấm hình lên GUI
                BitmapImage bm = new BitmapImage();
                FileStream f = new FileStream(tenfileHinhTemp, FileMode.Open); // liên kết hình trên đĩa trc
                bm.BeginInit();
                bm.StreamSource = f; // liên kết bm vs file stream
                bm.EndInit();

                imgHinh.Source = bm;
            }
        }

        private void btnBochonhinh_Click(object sender, RoutedEventArgs e)
        {
            // lá cờ bẫy cho change img khi upd
            flagChange = true;

            tenfileHinhTemp = "";
            if (imgHinh.Source == null) return;
            // ngắt connect tới file để bỏ hình đc chọn
            BitmapImage bmTemp = imgHinh.Source as BitmapImage;
            if (bmTemp != null)
            {
                bmTemp.StreamSource.Close();
                imgHinh.Source = null;
            }
        }

        private void rdoSua_Click(object sender, RoutedEventArgs e)
        {
            txtMshv.IsReadOnly = true;
        }

        private void rdoXoa_Click(object sender, RoutedEventArgs e)
        {
            txtMshv.IsReadOnly = true;
        }

        private void rdoThem_Click(object sender, RoutedEventArgs e)
        {
            txtMshv.IsReadOnly = true;
        }
    }
}
