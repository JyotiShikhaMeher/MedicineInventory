
const medicineApi = "/api/medicines";
const salesApi = "/api/sales";

let medicines = [];
let searchTimer;

document.addEventListener("DOMContentLoaded", () => {
    loadMedicines();
    loadSales();

    document.getElementById("medicineForm")
        .addEventListener("submit", addMedicine);

    document.getElementById("saleForm")
        .addEventListener("submit", recordSale);

    document.getElementById("searchInput")
        .addEventListener("input", () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(loadMedicines, 250);
        });
});

async function apiRequest(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        throw new Error(error.message || `Request failed: ${response.status}`);
    }

    if (response.status === 204) {
        return null;
    }

    return response.json();
}

function showMessage(message, isError = false) {
    const element = document.getElementById("message");

    element.textContent = message;
    element.className = isError
        ? "alert alert-danger"
        : "alert alert-success";
}

function clearMessage() {
    document.getElementById("message").className = "alert d-none";
}

async function loadMedicines() {
    const search = document.getElementById("searchInput").value.trim();
    const url = search
        ? `${medicineApi}?search=${encodeURIComponent(search)}`
        : medicineApi;

    try {
        medicines = await apiRequest(url);
        renderMedicines(medicines);
        populateSaleDropdown(medicines);
    } catch (error) {
        showMessage(error.message, true);
    }
}

function renderMedicines(items) {
    const tbody = document.getElementById("medicineTableBody");
    tbody.replaceChildren();

    if (items.length === 0) {
        const row = tbody.insertRow();
        const cell = row.insertCell();
        cell.colSpan = 7;
        cell.textContent = "No medicines found.";
        return;
    }

    items.forEach(medicine => {
        const row = tbody.insertRow();

        const daysLeft = Math.ceil(
            (new Date(medicine.expiryDate).setHours(0, 0, 0, 0) -
                new Date().setHours(0, 0, 0, 0)) /
            (1000 * 60 * 60 * 24)
        );

        if (daysLeft < 30) {
            row.classList.add("expiry-warning");
        } else if (medicine.quantity < 10) {
            row.classList.add("stock-warning");
        }

        addCell(row, medicine.id);
        addCell(row, medicine.fullName);
        addCell(row, medicine.brand);
        addCell(row, formatDate(medicine.expiryDate));
        addCell(row, medicine.quantity);
        addCell(row, Number(medicine.price).toFixed(2));

        const actionCell = row.insertCell();

        const deleteButton = document.createElement("button");
        deleteButton.type = "button";
        deleteButton.className = "btn btn-sm btn-danger";
        deleteButton.textContent = "Delete";
        deleteButton.addEventListener("click", () => deleteMedicine(medicine.id));

        actionCell.appendChild(deleteButton);
    });
}

function addCell(row, value) {
    const cell = row.insertCell();
    cell.textContent = value ?? "";
}

function formatDate(value) {
    if (!value) return "";

    const date = new Date(value);

    return Number.isNaN(date.getTime())
        ? ""
        : date.toLocaleDateString();
}

function populateSaleDropdown(items) {
    const dropdown = document.getElementById("saleMedicineId");
    const previousValue = dropdown.value;

    dropdown.replaceChildren();

    const placeholder = document.createElement("option");
    placeholder.value = "";
    placeholder.textContent = "Select medicine";
    dropdown.appendChild(placeholder);

    items.forEach(medicine => {
        const option = document.createElement("option");
        option.value = medicine.id;
        option.textContent =
            `${medicine.fullName} (Stock: ${medicine.quantity})`;

        dropdown.appendChild(option);
    });

    if (items.some(m => String(m.id) === previousValue)) {
        dropdown.value = previousValue;
    }
}

async function addMedicine(event) {
    event.preventDefault();
    clearMessage();

    const priceInput = document.getElementById("price");
    const quantityInput = document.getElementById("quantity");
    const price = Number(priceInput.value);
    const quantity = Number(quantityInput.value);

    if (!Number.isFinite(price) || price < 0 ||
        !Number.isFinite(quantity) || !Number.isInteger(quantity) ||
        quantity < 0) {
        showMessage("Enter a valid price and whole-number quantity.", true);
        return;
    }

    if (Math.round(price * 100) !== price * 100) {
        showMessage("Price must have no more than 2 decimal places.", true);
        return;
    }

    const medicine = {
        fullName: document.getElementById("fullName").value.trim(),
        brand: document.getElementById("brand").value.trim(),
        expiryDate: document.getElementById("expiryDate").value,
        quantity: quantity,
        price: price,
        notes: document.getElementById("notes").value.trim()
    };

    if (!medicine.fullName || !medicine.brand || !medicine.expiryDate) {
        showMessage("Complete all required fields.", true);
        return;
    }

    try {
        await apiRequest(medicineApi, {
            method: "POST",
            body: JSON.stringify(medicine)
        });

        document.getElementById("medicineForm").reset();
        showMessage("Medicine added successfully.");
        await loadMedicines();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function deleteMedicine(id) {
    if (!confirm("Are you sure you want to delete this medicine?")) {
        return;
    }

    try {
        await apiRequest(`${medicineApi}/${id}`, {
            method: "DELETE"
        });

        showMessage("Medicine deleted successfully.");
        await loadMedicines();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function recordSale(event) {
    event.preventDefault();
    clearMessage();

    const medicineId = Number(
        document.getElementById("saleMedicineId").value
    );
    const quantitySold = Number(
        document.getElementById("saleQuantity").value
    );

    if (!medicineId || !Number.isInteger(quantitySold) ||
        quantitySold <= 0) {
        showMessage("Select a medicine and enter a valid quantity.", true);
        return;
    }

    try {
        await apiRequest(salesApi, {
            method: "POST",
            body: JSON.stringify({ medicineId, quantitySold })
        });

        document.getElementById("saleForm").reset();
        showMessage("Sale recorded successfully.");

        await loadMedicines();
        await loadSales();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function loadSales() {
    try {
        const sales = await apiRequest(salesApi);
        renderSales(sales);
    } catch (error) {
        showMessage(error.message, true);
    }
}

function renderSales(sales) {
    const tbody = document.getElementById("salesTableBody");
    tbody.replaceChildren();

    if (sales.length === 0) {
        const row = tbody.insertRow();
        const cell = row.insertCell();
        cell.colSpan = 6;
        cell.textContent = "No sales recorded yet.";
        return;
    }

    sales.forEach(sale => {
        const row = tbody.insertRow();

        addCell(row, sale.id);
        addCell(row, sale.medicineName);
        addCell(row, sale.quantitySold);
        addCell(row, Number(sale.unitPrice).toFixed(2));
        addCell(row, Number(sale.totalAmount).toFixed(2));
        addCell(row, new Date(sale.saleDate).toLocaleString());
    });
}
