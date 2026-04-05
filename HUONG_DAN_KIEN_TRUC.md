# 📚 Hướng Dẫn Kiến Trúc Dự Án SV22T1020065

## 🏗️ Tổng Quan Kiến Trúc

Dự án này sử dụng **kiến trúc N-Tier (Tầng)** gồm 4 tầng chính:

```
┌─────────────────────────────────────────┐
│    PRESENTATION LAYER (Tầng Giao Diện)  │
│  Admin / Shop (Controllers, Views)      │
├─────────────────────────────────────────┤
│   BUSINESS LAYER (Tầng Nghiệp Vụ)       │
│   BusinessLayers (DataService)          │
├─────────────────────────────────────────┤
│    DATA LAYER (Tầng Dữ Liệu)           │
│  DataLayers (Repository, Interface)     │
├─────────────────────────────────────────┤
│     DOMAIN LAYER (Tầng Dữ Liệu)        │
│  Models (Thực thể và ViewModel)         │
└─────────────────────────────────────────┘
```

---

## 📂 Cấu Trúc Thư Mục Chi Tiết

### 1️⃣ **SV22T1020065.Models** - Tầng Tên Miền (Domain Layer)

**Mục đích:** Chứa tất cả các class mô tả dữ liệu của ứng dụng

#### Cấu trúc thư mục:
```
Models/
├── Catalog/           # Lĩnh vực Quản lý Sản Phẩm
├── Sales/            # Lĩnh vực Bán Hàng
├── Common/           # Dữ liệu chung
└── Security/         # Bảo mật
```

#### Chi tiết các thư mục:

**📦 Models/Catalog/** - Quản lý Sản Phẩm
- `Category.cs` - Thông tin Loại sản phẩm
- `Product.cs` - Thông tin Sản phẩm
- `CategoryViewInfo.cs` - Dữ liệu gửi đến View để hiển thị loại sản phẩm
- `ProductViewInfo.cs` - Dữ liệu gửi đến View để hiển thị sản phẩm

**📦 Models/Sales/** - Quản lý Bán Hàng
- `Order.cs` - Đơn hàng (dữ liệu từ DB)
- `OrderDetail.cs` - Chi tiết đơn hàng (dữ liệu từ DB)
- `OrderViewInfo.cs` - Dữ liệu hiển thị đơn hàng trên View
- `OrderDetailViewInfo.cs` - Dữ liệu hiển thị chi tiết đơn hàng
- `OrderDetailsViewModel.cs` - ViewModel kết hợp Order + danh sách OrderDetail
- `OrderSearchInput.cs` - Điều kiện tìm kiếm đơn hàng
- `OrderStatusEnum.cs` - Enum trạng thái đơn hàng
- `OrderStatusExtensions.cs` - Phương thức mở rộng cho enum

**📦 Models/Common/** - Dữ liệu Chung
- `PagedResult<T>` - Kết quả phân trang cho danh sách
- `PageItem.cs` - Một item trong danh sách phân trang
- `PaginationSearchInput.cs` - Input tìm kiếm có phân trang

**📦 Models/Security/** - Bảo Mật
- `UserAccount.cs` - Tài khoản người dùng
- `AccountProfile.cs` - Hồ sơ tài khoản

---

### 2️⃣ **SV22T1020065.DataLayers** - Tầng Dữ Liệu (Data Layer)

**Mục đích:** Kết nối và thao tác dữ liệu từ Database

#### Cấu trúc:
```
DataLayers/
├── DataLayers.Interfaces/      # Định nghĩa Interface
│   ├── Interfaces/
│   │   └── I[Entity]Repository.cs   # Interface Repository
│   └── Repository/
│       └── [Entity]Repository.cs    # Implement Repository
```

#### Các Repository chính:

| Repository | Mục đích |
|-----------|---------|
| `OrderRepository.cs` | Thao tác Order trong DB (Select, Insert, Update, Delete) |
| `CustomerRepository.cs` | Quản lý thông tin khách hàng |
| `ProductRepository.cs` | Quản lý thông tin sản phẩm, hình ảnh |
| `CategoryRepository.cs` | Quản lý loại sản phẩm |
| `EmployeeRepository.cs` | Quản lý nhân viên |
| `SupplierRepository.cs` | Quản lý nhà cung cấp |
| `ShipperRepository.cs` | Quản lý đơn vị vận chuyển |
| `ProvinceRepository.cs` | Quản lý Tỉnh/Thành phố |
| `UserAccountRepository.cs` | Quản lý tài khoản người dùng |

**💡 Ví dụ: OrderRepository làm gì?**
```
OrderRepository
│
├── ListAsync(OrderSearchInput) → PagedResult<OrderViewInfo>
│   (Tìm kiếm đơn hàng theo điều kiện)
│
├── GetAsync(int orderID) → OrderViewInfo
│   (Lấy thông tin 1 đơn hàng cụ thể)
│
├── InsertAsync(Order) → int
│   (Tạo đơn hàng mới)
│
├── UpdateAsync(Order) → bool
│   (Cập nhật thông tin đơn hàng)
│
└── DeleteAsync(int orderID) → bool
    (Xóa một đơn hàng)
```

---

### 3️⃣ **SV22T1020065.BusinessLayers** - Tầng Nghiệp Vụ (Business Layer)

**Mục đích:** Xử lý logic kinh doanh, kết nối giữa UI và Data Layer

#### Các DataService chính:

| Service | Mục đích |
|---------|---------|
| `SalesDataService.cs` | Xử lý logic bán hàng (Order, OrderDetail) |
| `CatalogDataService.cs` | Xử lý sản phẩm, loại sản phẩm |
| `HRDataService.cs` | Xử lý nhân sự (Employee) |
| `PartnerDataService.cs` | Xử lý đối tác (Customer, Supplier, Shipper) |
| `DictionaryDataService.cs` | Xử lý dữ liệu công cộng (Tỉnh/Thành) |

**💡 Ví dụ: SalesDataService làm gì?**
```
SalesDataService
│
├── ListOrdersAsync(OrderSearchInput)
│   └── Gọi → OrderRepository.ListAsync()
│
├── GetOrderAsync(int orderID)
│   └── Gọi → OrderRepository.GetAsync()
│
├── CreateOrderAsync(Order)
│   ├── Kiểm tra logic (Validate)
│   ├── Gọi → OrderRepository.InsertAsync()
│   └── Trả về kết quả
│
└── ... (các method khác)
```

**⚡ Qui trình:**
1. Controller gọi SalesDataService
2. SalesDataService kiểm tra logic business
3. SalesDataService gọi OrderRepository
4. OrderRepository thực thi SQL query
5. Dữ liệu được trả về và xử lý

---

### 4️⃣ **SV22T1020065.Admin** - Tầng Giao Diện (Presentation Layer)

**Mục đích:** Hiển thị UI cho người dùng, nhận request và gọi BusinessLayer

#### Cấu trúc:
```
Admin/
├── Controllers/        # Xử lý request từ user
├── Views/             # Giao diện HTML
├── AppCodes/          # Các utility, helper class
├── wwwroot/           # CSS, JS, images
└── Properties/        # Cấu hình ứng dụng
```

**📝 Controllers - Xử lý Request:**

| Controller | Mục đích | Các Action Chính |
|-----------|---------|-----------------|
| `OrderController.cs` | Quản lý đơn hàng | Index, Create, Edit, Delete, Search |
| `ProductController.cs` | Quản lý sản phẩm | Index, Create, Edit, Delete |
| `CategoryController.cs` | Quản lý loại sản phẩm | Index, Create, Edit, Delete |
| `CustomerController.cs` | Quản lý khách hàng | Index, Create, Edit, Delete |
| `EmployeeController.cs` | Quản lý nhân viên | Index, Create, Edit, Delete |
| `SupplierController.cs` | Quản lý nhà cung cấp | Index, Create, Edit, Delete |
| `ShipperController.cs` | Quản lý đơn vị vận chuyển | Index, Create, Edit, Delete |
| `AccountController.cs` | Quản lý tài khoản | Login, Logout, Register |
| `HomeController.cs` | Trang chủ | Index, Dashboard |

**💡 Ví dụ: OrderController.Index() làm gì?**
```csharp
public async Task<IActionResult> Index(OrderSearchInput input)
{
    // 1. Nhận dữ liệu từ URL/Form
    // 2. Gọi SalesDataService.ListOrdersAsync(input)
    // 3. SalesDataService gọi OrderRepository
    // 4. OrderRepository trả về danh sách Order từ DB
    // 5. Controller gửi dữ liệu đến View
    // 6. View hiển thị danh sách lên website
}
```

**🎨 Views - Giao Diện HTML:**
```
Views/
├── Order/
│   ├── Index.cshtml          # Danh sách đơn hàng
│   ├── Create.cshtml         # Form tạo đơn hàng
│   ├── Edit.cshtml           # Form sửa đơn hàng
│   └── Details.cshtml        # Chi tiết 1 đơn hàng
├── Product/                  # Tương tự
├── Category/                 # Tương tự
├── Shared/                   # Layout chung cho toàn site
└── _ViewStart.cshtml         # Cấu hình View chung
```

**🛠️ AppCodes/ - Các Helper và Utility:**

| File | Mục đích |
|------|---------|
| `ApplicationContext.cs` | Quản lý session, cấu hình ứng dụng |
| `ApiResult.cs` | Format kết quả trả về API |
| `CryptHelper.cs` | Mã hóa/giải mã dữ liệu |
| `SelectListHelper.cs` | Tạo dropdown list cho View |
| `WebSecurityModels.cs` | Xử lý phân quyền, bảo mật |
| `ShopingCartService.cs` | Quản lý giỏ hàng |

---

## 🔄 Qui Trình Dữ Liệu (Data Flow)

### Ví dụ: Hiển thị danh sách đơn hàng

```
1. USER nhấp "Xem Đơn Hàng" trên website
                    ↓
2. Browser gửi Request: GET /Order/Index
                    ↓
3. OrderController.Index() nhận request
                    ↓
4. Controller gọi:
   SalesDataService.ListOrdersAsync(input)
                    ↓
5. SalesDataService gọi:
   OrderRepository.ListAsync(input)
                    ↓
6. OrderRepository:
   - Nhận input (tìm kiếm, lọc, phân trang)
   - Viết SQL query
   - Kết nối Database
   - Thực thi query
   - Lấy kết quả từ DB
   - Map sang OrderViewInfo
   - Trả về PagedResult<OrderViewInfo>
                    ↓
7. SalesDataService trả về kết quả cho Controller
                    ↓
8. Controller gửi dữ liệu đến View:
   ActionResult(result)
                    ↓
9. Views/Order/Index.cshtml:
   - Nhận PagedResult<OrderViewInfo>
   - Loop qua từng Order
   - Tạo HTML table
   - Render lên browser
                    ↓
10. Browser hiển thị trang web cho User
```

---

## 📊 Mối Quan Hệ Giữa Các Model

```
┌──────────────────────────────────────────────┐
│         ORDER (Đơn Hàng)                     │
├──────────────────────────────────────────────┤
│ - OrderID (Mã đơn hàng)                      │
│ - CustomerID (Mã khách hàng)                 │
│ - OrderDate (Ngày đặt)                       │
│ - DeliveryDate (Ngày giao)                   │
│ - Status (Trạng thái)                        │
│ - Description (Ghi chú)                      │
└──────────────────────────────────────────────┘
            │
            │ 1 Order có nhiều OrderDetail
            │
            ↓
┌──────────────────────────────────────────────┐
│      ORDER DETAIL (Chi Tiết Đơn Hàng)        │
├──────────────────────────────────────────────┤
│ - OrderDetailID                              │
│ - OrderID (FK: Tham chiếu Order)            │
│ - ProductID (FK: Tham chiếu Product)        │
│ - Quantity (Số lượng)                        │
│ - UnitPrice (Giá tiền)                       │
└──────────────────────────────────────────────┘
            │
            │ OrderDetail tham chiếu Product
            │
            ↓
┌──────────────────────────────────────────────┐
│       PRODUCT (Sản Phẩm)                     │
├──────────────────────────────────────────────┤
│ - ProductID (Mã sản phẩm)                    │
│ - CategoryID (FK: Loại sản phẩm)            │
│ - ProductName (Tên sản phẩm)                │
│ - Price (Giá bán)                           │
│ - Photo (Hình ảnh)                          │
└──────────────────────────────────────────────┘
```

---

## 🔍 Chi Tiết: Từ Model → Repository → Service → Controller

### Lấy ví dụ: QUẢN LÝ ĐƠN HÀNG

**🎯 Mục đích:** Hiển thị danh sách đơn hàng với tìm kiếm và phân trang

---

### Step 1: Models (SV22T1020065.Models)

**Order.cs** - Thực thể từ Database:
```csharp
public class Order
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public OrderStatusEnum Status { get; set; }
    public string Description { get; set; }
}
```

**OrderDetail.cs** - Chi tiết đơn hàng:
```csharp
public class OrderDetail
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

**OrderSearchInput.cs** - Input tìm kiếm:
```csharp
public class OrderSearchInput : PaginationSearchInput
{
    public string CustomerName { get; set; }
    public OrderStatusEnum Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
```

**OrderViewInfo.cs** - Dữ liệu gửi View:
```csharp
public class OrderViewInfo
{
    public int OrderID { get; set; }
    public string CustomerName { get; set; }
    public int OrderDetails { get; set; }  // Số chi tiết
    public decimal Total { get; set; }      // Tổng tiền
    public OrderStatusEnum Status { get; set; }
    public DateTime OrderDate { get; set; }
}
```

---

### Step 2: Data Layer (SV22T1020065.DataLayers)

**IOrderRepository.cs** - Interface (định nghĩa what):
```csharp
public interface IOrderRepository
{
    Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input);
    Task<OrderViewInfo> GetAsync(int orderID);
    Task<int> InsertAsync(Order order);
    Task<bool> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int orderID);
}
```

**OrderRepository.cs** - Implementation (thực thi how):
```csharp
public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;
    
    public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
    {
        var items = new List<OrderViewInfo>();
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            // Viết SQL query:
            // SELECT o.OrderID, c.CustomerName, 
            //        COUNT(*) as OrderDetails,
            //        SUM(od.Quantity * od.UnitPrice) as Total,
            //        o.Status, o.OrderDate
            // FROM Orders o
            // JOIN Customers c ON o.CustomerID = c.CustomerID
            // JOIN OrderDetails od ON o.OrderID = od.OrderID
            // WHERE ... (filter theo input)
            // ORDER BY ... (sort)
            // OFFSET ... ROWS FETCH NEXT ... ROWS (phân trang)
            
            using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerName", 
                    input.CustomerName ?? "");
                cmd.Parameters.AddWithValue("@Status", 
                    input.Status);
                // ... add more parameters
                
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new OrderViewInfo
                    {
                        OrderID = (int)reader["OrderID"],
                        CustomerName = reader["CustomerName"].ToString(),
                        // ... map fields
                    });
                }
            }
        }
        
        return new PagedResult<OrderViewInfo>
        {
            Data = items,
            PageIndex = input.Page,
            PageSize = input.PageSize,
            TotalRecords = totalCount
        };
    }
}
```

---

### Step 3: Business Layer (SV22T1020065.BusinessLayers)

**SalesDataService.cs** - Xử lý logic:
```csharp
public static class SalesDataService
{
    private static readonly IOrderRepository orderDB;
    
    static SalesDataService()
    {
        // Khởi tạo repository
        orderDB = new OrderRepository(
            Configuration.ConnectionString);
    }
    
    /// <summary>
    /// Lấy danh sách đơn hàng với tìm kiếm và phân trang
    /// </summary>
    public static async Task<PagedResult<OrderViewInfo>> 
        ListOrdersAsync(OrderSearchInput input)
    {
        // Validate input
        if (input.Page < 1) input.Page = 1;
        if (input.PageSize < 1) input.PageSize = 10;
        
        // Gọi Repository để lấy dữ liệu
        var result = await orderDB.ListAsync(input);
        
        // Có thể thêm logic xử lý nếu cần
        // (Ví dụ: tính lại tổng, format dữ liệu, etc.)
        
        return result;
    }
}
```

**💡 Tại sao cần Business Layer?**
- Kiểm tra logic nghiệp vụ (Validation)
- Xử lý dữ liệu trước khi trả về
- Quản lý transaction (nếu cần)
- Tái sử dụng code ở nhiều Controller

---

### Step 4: Presentation Layer (SV22T1020065.Admin)

**OrderController.cs** - Handle Request:
```csharp
[Authorize(Roles = "Administrator,DataManager")]
public class OrderController : Controller
{
    private const string OrderSearch = "OrderSearchProductInput";
    
    public async Task<IActionResult> Index(
        string customerName = "",
        OrderStatusEnum? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        // 1. Chuẩn bị input tìm kiếm
        var input = new OrderSearchInput
        {
            CustomerName = customerName,
            Status = status ?? OrderStatusEnum.New,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = 1,
            PageSize = 10
        };
        
        // 2. Gọi SalesDataService để lấy dữ liệu
        var result = await SalesDataService.ListOrdersAsync(input);
        
        // 3. Lưu input vào Session (để nhớ filter)
        ApplicationContext.SetSessionData(OrderSearch, input);
        
        // 4. Gửi dữ liệu đến View
        return View(result);
    }
}
```

**Views/Order/Index.cshtml** - Hiển thị dữ liệu:
```html
@model PagedResult<OrderViewInfo>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Mã Đơn</th>
            <th>Khách Hàng</th>
            <th>Ngày Đặt</th>
            <th>Trạng Thái</th>
            <th>Tổng Tiền</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model.Data)
        {
            <tr>
                <td>@item.OrderID</td>
                <td>@item.CustomerName</td>
                <td>@item.OrderDate.ToString("dd/MM/yyyy")</td>
                <td>
                    <span class="badge">@item.Status</span>
                </td>
                <td>@item.Total.ToString("C")</td>
                <td>
                    <a href="/Order/Edit/@item.OrderID">
                        Sửa
                    </a>
                    <a href="/Order/Delete/@item.OrderID">
                        Xóa
                    </a>
                </td>
            </tr>
        }
    </tbody>
</table>

<!-- Phân trang -->
<nav>
    <ul class="pagination">
        @for (int i = 1; i <= Model.TotalPages; i++)
        {
            <li>
                <a href="?page=@i">@i</a>
            </li>
        }
    </ul>
</nav>
```

---

## 🎓 Ví Dụ Cụ Thể: Thêm sản phẩm vào giỏ hàng

### Qui trình:

1. **User click nút "Thêm vào giỏ"** trên View
   ```html
   <form method="post" action="/ShoppingCart/Add">
       <input type="hidden" name="ProductID" value="@product.ProductID">
       <input type="number" name="Quantity" value="1">
       <button type="submit">Thêm vào giỏ</button>
   </form>
   ```

2. **Controller nhận request**
   ```csharp
   // ShoppingCartController.cs
   [HttpPost]
   public async Task<IActionResult> Add(int productID, int quantity)
   {
       // Lấy thông tin sản phẩm từ Database
       var product = await CatalogDataService
           .GetProductAsync(productID);
       
       // Lấy giỏ hàng từ Session
       var cart = ApplicationContext
           .GetSessionData<ShoppingCart>("Cart") 
           ?? new ShoppingCart();
       
       // Thêm sản phẩm vào giỏ
       cart.AddItem(product, quantity);
       
       // Lưu giỏ hàng vào Session
       ApplicationContext.SetSessionData("Cart", cart);
       
       return RedirectToAction("Index");
   }
   ```

3. **Service gọi Repository**
   ```csharp
   // CatalogDataService
   public static async Task<ProductViewInfo> 
       GetProductAsync(int productID)
   {
       return await productDB.GetAsync(productID);
   }
   ```

4. **Repository truy vấn Database**
   ```csharp
   // ProductRepository
   public async Task<ProductViewInfo> GetAsync(int productID)
   {
       // SQL: SELECT * FROM Products WHERE ProductID = @id
       
       using (SqlCommand cmd = new SqlCommand(sql, conn))
       {
           cmd.Parameters.AddWithValue("@id", productID);
           var reader = await cmd.ExecuteReaderAsync();
           // ... Map dữ liệu
       }
   }
   ```

5. **View hiển thị giỏ hàng**
   ```html
   @{
       var cart = ApplicationContext
           .GetSessionData<ShoppingCart>("Cart");
   }
   
   <table>
       @foreach (var item in cart.Items)
       {
           <tr>
               <td>@item.ProductName</td>
               <td>@item.Quantity</td>
               <td>@(item.Price * item.Quantity)</td>
           </tr>
       }
   </table>
   ```

---

## 🚀 Best Practices & Quy Tắc

### ✅ Đúng Cách Làm:

```csharp
// Controller chỉ gọi Service
var result = await SalesDataService.ListOrdersAsync(input);

// Service xử lý logic, gọi Repository
public static async Task<PagedResult<OrderViewInfo>> 
    ListOrdersAsync(OrderSearchInput input)
{
    // Validate
    if (input.Page < 1) input.Page = 1;
    
    // Call Repository
    return await orderDB.ListAsync(input);
}

// Repository chỉ làm việc với Database
public async Task<PagedResult<OrderViewInfo>> ListAsync(...)
{
    // SQL query
    // Map to Model
    // Return result
}
```

### ❌ Sai Cách Làm:

```csharp
// ❌ Controller gọi trực tiếp Database
var result = await orderDB.ListAsync(input);

// ❌ Repository chứa logic nghiệp vụ phức tạp
// ❌ View có logic xử lý dữ liệu
// ❌ Không validate input
```

---

## 📋 Checklist: Tạo Tính Năng Mới

Nếu bạn muốn thêm tính năng mới, làm theo các bước sau:

### 1. Tạo Model (Models/)
```csharp
// SV22T1020065.Models/Models/Sales/NewEntity.cs
public class NewEntity
{
    public int ID { get; set; }
    // ... properties
}
```

### 2. Tạo ViewModel (Models/)
```csharp
public class NewEntityViewInfo
{
    public int ID { get; set; }
    // ... properties for display
}
```

### 3. Tạo Input Model (Models/)
```csharp
public class NewEntitySearchInput : PaginationSearchInput
{
    public string SearchField { get; set; }
}
```

### 4. Tạo Repository (DataLayers/)
```csharp
// Interface
public interface INewEntityRepository
{
    Task<PagedResult<NewEntityViewInfo>> ListAsync(
        NewEntitySearchInput input);
    Task<NewEntityViewInfo> GetAsync(int id);
    Task<int> InsertAsync(NewEntity entity);
    Task<bool> UpdateAsync(NewEntity entity);
    Task<bool> DeleteAsync(int id);
}

// Implementation
public class NewEntityRepository : INewEntityRepository
{
    // Thực thi các method
}
```

### 5. Thêm vào Service (BusinessLayers/)
```csharp
public static class CatalogDataService
{
    private static readonly INewEntityRepository newEntityDB;
    
    public static async Task<PagedResult<NewEntityViewInfo>>
        ListNewEntitiesAsync(NewEntitySearchInput input)
    {
        return await newEntityDB.ListAsync(input);
    }
}
```

### 6. Tạo Controller (Admin/Controllers/)
```csharp
public class NewEntityController : Controller
{
    public async Task<IActionResult> Index()
    {
        var input = new NewEntitySearchInput { Page = 1 };
        var result = await CatalogDataService
            .ListNewEntitiesAsync(input);
        return View(result);
    }
}
```

### 7. Tạo Views (Admin/Views/NewEntity/)
```html
@model PagedResult<NewEntityViewInfo>
<!-- Hiển thị dữ liệu -->
```

---

## 📞 Tóm Tắt Nhanh

| Lớp | Tập Tin | Mục Đích |
|-----|---------|---------|
| **Domain** | Models/*.cs | Định nghĩa cấu trúc dữ liệu |
| **Data** | DataLayers/*.cs | Truy vấn Database |
| **Business** | BusinessLayers/*.cs | Xử lý logic |
| **Presentation** | Admin/Controllers/*.cs | Nhận request, gửi response |
| **Presentation** | Admin/Views/*.cshtml | Hiển thị UI |

---

**Tác giả:** Hệ thống hướng dẫn tự động  
**Ngày cập nhật:** 2026-04-05  
**Phiên bản dự án:** SV22T1020065
