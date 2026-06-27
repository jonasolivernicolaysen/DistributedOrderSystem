import { useState, useEffect } from "react";
import { toast } from "react-toastify";

interface UserDetails {
    username: string;
    email: string;
    balance: number;
}

interface Product {
    productId: string;
    name: string;
    description: string;
    price: number;
}

function ProfilePage() {
    const [details, setDetails] = useState<UserDetails | null>(null);
    const [products, setProducts] = useState<Product[]>([]);
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

    const [showUpdateListing, setShowUpdateListing] = useState(false);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [editPrice, setEditPrice] = useState<number | "">(0);

    const [showUpdateStock, setShowUpdateStock] = useState(false);
    const [newStock, setNewStock] = useState(0);

    const [showDelete, setShowDelete] = useState(false);

    const updateListing = async (productId: string) => {
        const token = localStorage.getItem("token");
        const response = await fetch(`https://localhost:7144/api/products/${productId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                productId: selectedProduct?.productId,
                name: editName,
                description: editDescription,
                price: editPrice
            })
        });

        if (!response.ok) {
            toast.error(`Request failed: ${response.status}`);
            return;
        }

        toast.success("Product listing updated successfully");
        setShowUpdateListing(false);
    };

    const updateStock = async (productId: string, updatedStock: number) => {
        const token = localStorage.getItem("token");
        const response = await fetch(`https://localhost:7144/api/inventory/${productId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ updatedStock })
        });

        if (!response.ok) {
            toast.error(`Request failed: ${response.status}`);
            return;
        }

        toast.success("Stock updated successfully");
        setShowUpdateStock(false);
    };

    const deleteListing = async (productId: string) => {
        const token = localStorage.getItem("token");
        const response = await fetch(`https://localhost:7144/api/products/${productId}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ productId })
        });

        if (!response.ok) {
            toast.error(`Request failed: ${response.status}`);
            return;
        }

        toast.success("Successfully deleted product");
        setShowDelete(false);
        setProducts(products.filter(p => p.productId !== productId));
    };

    useEffect(() => {
        const getUserDetails = async () => {
            const token = localStorage.getItem("token");
            const response = await fetch("https://localhost:7144/api/auth/profile", {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                }
            });

            if (!response.ok) {
                toast.error(`Request failed: ${response.status}`);
                return;
            }

            setDetails(await response.json());
        };

        getUserDetails();
    }, []);

    useEffect(() => {
        const getUserProducts = async () => {
            const token = localStorage.getItem("token");
            const response = await fetch("https://localhost:7144/api/products/me", {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                }
            });

            if (!response.ok) {
                toast.error(`Request failed: ${response.status}`);
                return;
            }

            setProducts(await response.json());
        };

        getUserProducts();
    }, []);

    return (
        <div className="container mt-4">

            {/* Profile header */}
            <div className="card shadow-sm mb-4">
                <div className="card-body">
                    <h5 className="card-title mb-3">Profile</h5>
                    <div className="row justify-content-center text-center text-muted small">
                        <div className="col-auto">
                            <span className="fw-semibold text-dark">Username</span>
                            <p className="mb-0">{details?.username ?? "—"}</p>
                        </div>
                        <div className="col-auto">
                            <span className="fw-semibold text-dark">Email</span>
                            <p className="mb-0">{details?.email ?? "—"}</p>
                        </div>
                        <div className="col-auto">
                            <span className="fw-semibold text-dark">Balance</span>
                            <p className="mb-0">${details?.balance.toFixed(2) ?? "—"}</p>
                        </div>
                    </div>
                </div>
            </div>

            {/* My listings */}
            <h6 className="fw-semibold mb-3">My Listings</h6>
            <div className="row">
                {products.length === 0 && (
                    <p className="text-muted small">You have no product listings yet.</p>
                )}

                {products.map((product: Product) => {
                    const isSelected = selectedProduct?.productId === product.productId;

                    return (
                        <div key={product.productId} className="col-md-4 mb-3">
                            <div
                                className={`card shadow-sm ${isSelected ? "border-primary" : ""}`}
                                style={{ cursor: "pointer" }}
                                onClick={() => setSelectedProduct(isSelected ? null : product)}
                            >
                                <div className="card-body">
                                    <h6 className="card-title mb-1">{product.name}</h6>
                                    <span className="text-muted small">${product.price.toFixed(2)}</span>
                                </div>

                                {isSelected && (
                                    <div className="card-body border-top pt-3" onClick={(e) => e.stopPropagation()}>
                                        <p className="text-muted small mb-3">{product.description}</p>
                                        <div className="d-flex gap-2">
                                            <button
                                                className="btn btn-outline-primary btn-sm flex-grow-1"
                                                onClick={() => {
                                                    setEditName(product.name);
                                                    setEditDescription(product.description);
                                                    setEditPrice(product.price);
                                                    setShowUpdateListing(true);
                                                }}
                                            >
                                                Update
                                            </button>
                                            <button
                                                className="btn btn-outline-warning btn-sm flex-grow-1"
                                                onClick={() => setShowUpdateStock(true)}
                                            >
                                                Stock
                                            </button>
                                            <button
                                                className="btn btn-outline-danger btn-sm flex-grow-1"
                                                onClick={() => setShowDelete(true)}
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
            {/* Update Listing Modal */}
            {showUpdateListing && (
                <>
                    <div className="modal d-block">
                        <div className="modal-dialog">
                            <div className="modal-content">
                                <div className="modal-header">
                                    <h5 className="modal-title">Update Listing</h5>
                                </div>
                                <div className="modal-body">
                                    <div className="mb-3">
                                        <label className="form-label small fw-semibold">Name</label>
                                        <input
                                            className="form-control"
                                            value={editName}
                                            onChange={(e) => setEditName(e.target.value)}
                                        />
                                    </div>
                                    <div className="mb-3">
                                        <label className="form-label small fw-semibold">Description</label>
                                        <textarea
                                            className="form-control"
                                            value={editDescription}
                                            onChange={(e) => setEditDescription(e.target.value)}
                                            rows={3}
                                        />
                                    </div>
                                    <div className="mb-1">
                                        <label className="form-label small fw-semibold">Price</label>
                                        <div className="input-group">
                                            <input
                                                type="number"
                                                className="form-control"
                                                value={editPrice}
                                                min={1}
                                                step={0.01}
                                                onChange={(e) => setEditPrice(e.target.value === "" ? "" : Number(e.target.value))}
                                            />
                                            <span className="input-group-text">kr</span>
                                        </div>
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button className="btn btn-secondary btn-sm" onClick={() => setShowUpdateListing(false)}>Cancel</button>
                                    <button className="btn btn-primary btn-sm" onClick={() => selectedProduct && updateListing(selectedProduct.productId)}>Save</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="modal-backdrop fade show" />
                </>
            )}

            {/* Update Stock Modal */}
            {showUpdateStock && (
                <>
                    <div className="modal d-block">
                        <div className="modal-dialog">
                            <div className="modal-content">
                                <div className="modal-header">
                                    <h5 className="modal-title">Update Stock</h5>
                                </div>
                                <div className="modal-body">
                                    <label className="form-label small fw-semibold">New Stock</label>
                                    <input
                                        type="number"
                                        className="form-control"
                                        value={newStock}
                                        min={0}
                                        onChange={(e) => setNewStock(Number(e.target.value))}
                                    />
                                </div>
                                <div className="modal-footer">
                                    <button className="btn btn-secondary btn-sm" onClick={() => setShowUpdateStock(false)}>Cancel</button>
                                    <button className="btn btn-warning btn-sm" onClick={() => selectedProduct && updateStock(selectedProduct.productId, newStock)}>Save</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="modal-backdrop fade show" />
                </>
            )}

            {/* Delete Modal */}
            {showDelete && (
                <>
                    <div className="modal d-block">
                        <div className="modal-dialog">
                            <div className="modal-content">
                                <div className="modal-header">
                                    <h5 className="modal-title">Delete Listing</h5>
                                </div>
                                <div className="modal-body">
                                    Are you sure you want to delete <strong>{selectedProduct?.name}</strong>? This cannot be undone.
                                </div>
                                <div className="modal-footer">
                                    <button className="btn btn-secondary btn-sm" onClick={() => setShowDelete(false)}>Cancel</button>
                                    <button className="btn btn-danger btn-sm" onClick={() => selectedProduct && deleteListing(selectedProduct.productId)}>Delete</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="modal-backdrop fade show" />
                </>
            )}

        </div>
    );
}

export default ProfilePage;