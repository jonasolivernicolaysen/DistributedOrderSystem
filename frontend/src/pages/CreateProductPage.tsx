import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify"
import { apiFetch } from "../services/api"

function CreateProductPage() {
    const [productName, setProductName] = useState("");
    const [productDescription, setProductDescription] = useState("");
    const [productPrice, setProductPrice] = useState<number | "">("");

    const navigate = useNavigate();

    const addProductListing = async () => {
        try {
            const jwt = localStorage.getItem("jwt");
            const response = await apiFetch("http://localhost:7144/api/products", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${jwt}`
                },
                body: JSON.stringify({
                    name: productName,
                    description: productDescription,
                    price: productPrice
                })
            });

            if (!response.ok) {
                const error = await response.json();
                toast.error(error.error);
                return;
            }

            toast.success("Product listing added successfully");
            navigate("/products");
        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    return (
        <div className="container mt-4">
            <div className="row justify-content-center">
                <div className="col-md-4">

                    <button
                        className="btn btn-link ps-0 mb-3 text-decoration-none text-muted"
                        onClick={() => navigate("/products")}
                    >
                        ← Back to Products
                    </button>

                    <div className="card shadow-sm">
                        <div className="card-body">
                            <h5 className="card-title mb-4">Create Product</h5>

                            <div className="mb-3">
                                <label className="form-label small fw-semibold">Name</label>
                                <input
                                    className="form-control"
                                    value={productName}
                                    onChange={(e) => setProductName(e.target.value)}
                                    type="text"
                                    placeholder="e.g. Wireless Headphones"
                                />
                            </div>

                            <div className="mb-3">
                                <label className="form-label small fw-semibold">Description</label>
                                <textarea
                                    className="form-control"
                                    value={productDescription}
                                    onChange={(e) => setProductDescription(e.target.value)}
                                    placeholder="Brief product description"
                                    rows={3}
                                />
                            </div>

                            <div className="mb-4">
                                <label className="form-label small fw-semibold">Price</label>
                                <div className="input-group">
                                    <span className="input-group-text">$</span>
                                    <input
                                        className="form-control"
                                        value={productPrice}
                                        onChange={(e) => setProductPrice(e.target.value === "" ? "" : Number(e.target.value))}
                                        type="number"
                                        placeholder="1.00"
                                        min={1}
                                        step={0.01}
                                    />
                                </div>
                            </div>

                            <button
                                className="btn btn-primary w-100"
                                onClick={addProductListing}
                                disabled={!productName || !productDescription || productPrice === ""}
                            >
                                Add Product
                            </button>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    );
}

export default CreateProductPage;